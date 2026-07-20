using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Enums;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.Helpers;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.Servicios.Impl
{
    public class DriverIdentificacionVehicular : IDisposable
    {
        private ConfigIdentificacionVehicular config;
        private readonly Action<NotificacionEvento> notificar;
        private readonly ILogger log;

        private volatile IDriverLectorTarjetas driverTriggerLector;
        private volatile IDriverSensor driverSensorVehicular;
        private volatile IDriverSensor driverPresencia;
        private readonly SemaphoreSlim procesamientoSemaphore = new SemaphoreSlim(1, 1);

        // Snapshot en memoria — actualizado tras cada ProcesarTrigger
        private volatile string ultimoValorTarjeta;
        private bool? ultimaPresencia;
        private readonly Dictionary<string, DetalleCamaraCIV> ultimoDetallePorCamara
            = new Dictionary<string, DetalleCamaraCIV>(StringComparer.OrdinalIgnoreCase);
        private readonly object detalleLock = new object();

        // Buffer circular de últimas 50 notificaciones
        private readonly Queue<NotificacionCIVDto> bufferNotificaciones = new Queue<NotificacionCIVDto>(50);
        private readonly object bufferLock = new object();

        public DriverIdentificacionVehicular(
            ConfigIdentificacionVehicular config,
            Action<NotificacionEvento> notificar,
            ILogger log)
        {
            this.config = config;
            this.notificar = notificar;
            this.log = log;
        }

        public void AsignarTriggerDriverLectorTarjeta(IDriverLectorTarjetas driver)
        {
            if (driverTriggerLector != null)
            {
                driverTriggerLector.EventoDriver -= OnEventoTriggerLecturaTarjeta;
                log.Debug("[CIV:{0}] Lector de trigger desconectado.", config.Codigo);
            }

            driverTriggerLector = driver;

            if (driverTriggerLector != null)
            {
                driverTriggerLector.EventoDriver += OnEventoTriggerLecturaTarjeta;
                log.Debug("[CIV:{0}] Lector de trigger conectado.", config.Codigo);
            }
        }

        public void AsignarTriggerDriverSensor(IDriverSensor driver)
        {
            if (driverSensorVehicular != null)
            {
                driverSensorVehicular.EventoDriver -= OnEventoTriggerSensorVehicular;
                log.Debug("[CIV:{0}] Sensor vehicular desconectado.", config.Codigo);
            }

            driverSensorVehicular = driver;

            if (driverSensorVehicular != null)
            {
                driverSensorVehicular.EventoDriver += OnEventoTriggerSensorVehicular;
                log.Debug("[CIV:{0}] Sensor vehicular conectado.", config.Codigo);
            }
        }

        public void AsignarDriverPresencia(IDriverSensor driver)
        {
            driverPresencia = driver;
            log.Debug("[CIV:{0}] Sensor de presencia {1}.", config.Codigo, driver != null ? "conectado" : "desconectado");
        }

        private async void OnEventoTriggerLecturaTarjeta(object sender, EventoDriverEventArgs args)
        {
            var notificacion = args.Notificacion;
            if (notificacion.CodigoEvento != CodigosEventos.LecturaTarjetaRecibida)
                return;

            string valorTarjeta = string.Empty;
            notificacion.Datos?.TryGetValue(DatosNotificacion.Tarjeta, out valorTarjeta);
            log.Debug("[CIV:{0}] Trigger lector recibido - tarjeta:{1}.", config.Codigo, valorTarjeta);

            await ProcesarTrigger(valorTarjeta, "Tarjeta", new List<DetalleCamaraCIV>());
        }

        private async void OnEventoTriggerSensorVehicular(object sender, EventoDriverEventArgs args)
        {
            if (args.Notificacion.CodigoEvento != CodigosEventos.EntradaActivada)
                return;

            var detalles = ExtraerDetallesCamaras(args.Notificacion.Datos);
            await ProcesarTrigger(string.Empty, "Sensor", detalles);
        }

        private async Task ProcesarTrigger(string tarjeta, string trigger, List<DetalleCamaraCIV> detallesDelSensor)
        {
            await procesamientoSemaphore.WaitAsync();
            try
            {
                var fechaEvento = DateTime.Now;
                var estadoSensorPresencia = ConsultarPresencia();
                var resultadoCamaras = await ObtenerDetallesCamaras(detallesDelSensor);

                var datos = new Dictionary<string, string>
                {
                    [DatosNotificacion.Trigger] = trigger,
                    [DatosNotificacion.Tarjeta] = tarjeta,
                    [DatosNotificacion.FechaEvento] = fechaEvento.ToString("o"),
                    [DatosNotificacion.VehiculoPresente] = estadoSensorPresencia.ToString(),
                    [DatosNotificacion.Patente] = resultadoCamaras.Select(x => x.Patente).FirstOrDefault(patenteReconocida => !string.IsNullOrEmpty(patenteReconocida)),
                    [DatosNotificacion.Error] = resultadoCamaras.Select(x => x.Error).FirstOrDefault(error => !string.IsNullOrEmpty(error)),
                    [DatosNotificacion.Detalle] = resultadoCamaras.ToJson(),
                };

                notificar(new NotificacionEvento
                {
                    CodigoDispositivo = config.Codigo,
                    CodigoEvento = CodigosEventos.IdentificacionVehicular,
                    Datos = datos
                });

                // Actualizar snapshot
                ultimoValorTarjeta = tarjeta;
                ultimaPresencia = estadoSensorPresencia;
                lock (detalleLock)
                {
                    foreach (var d in resultadoCamaras)
                        ultimoDetallePorCamara[d.CodigoCamara] = d;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "[CIV:{0}] Error en ProcesarTrigger.", config.Codigo);
            }
            finally
            {
                procesamientoSemaphore.Release();
            }
        }

        private async Task<List<DetalleCamaraCIV>> ObtenerDetallesCamaras(List<DetalleCamaraCIV> detallesDelSensor)
        {
            foreach (var detalle in detallesDelSensor.Where(x => !string.IsNullOrEmpty(x.RutaImagen)))
                GuardarImageHikVision(detalle);

            if (detallesDelSensor.Any(d => !string.IsNullOrEmpty(d.Patente)))
                return detallesDelSensor;

            log.Debug("[CIV:{0}] Tomando fotos con ALPR (sin patente previa).", config.Codigo);
            var resultadoCamaras = await TomarFotosTodas();
            return detallesDelSensor.Concat(resultadoCamaras).ToList();
        }

        private async Task<List<DetalleCamaraCIV>> TomarFotosTodas()
        {
            var camaras = config.Camaras;
            if (camaras == null || !camaras.Any())
                return new List<DetalleCamaraCIV>();

            var tareas = camaras
                .Select(camaraCIV => Task.Run(() => CapturarPatente(camaraCIV.ConfigCamara)))
                .ToArray();

            return (await Task.WhenAll(tareas)).ToList();
        }

        private List<DetalleCamaraCIV> ExtraerDetallesCamaras(Dictionary<string, string> datos)
        {
            if (datos == null || !datos.TryGetValue(DatosNotificacion.Detalle, out var json) || string.IsNullOrEmpty(json))
                return new List<DetalleCamaraCIV>();

            try
            {
                return json.FromJson<List<DetalleCamaraCIV>>();
            }
            catch (Exception ex)
            {
                log.Warn(ex, "[CIV:{0}] No se pudo deserializar Detalle del evento del sensor.", config.Codigo);
                return new List<DetalleCamaraCIV>();
            }
        }

        private async Task<DetalleCamaraCIV> CapturarPatente(ConfigCamara camara)
        {
            int maxReintentos = config.MaxReintentosFoto > 0 ? config.MaxReintentosFoto : 0;
            int delayMs = config.DelayEntreReintentosMs;
            var codigoCamara = camara.Dispositivo?.Codigo;

            var detalle = new DetalleCamaraCIV
            {
                CodigoCamara = codigoCamara,
                ProveedorALPR = ProveedorALPR.OpenALPR.ToString()
            };

            for (int intento = 0; intento <= maxReintentos; intento++)
            {
                detalle.Intentos = intento + 1;
                try
                {
                    var (imagen, contentType) = ObtenerImagen(camara);
                    if (imagen == null)
                    {
                        log.Warn("[CIV:{0}] Cámara {1} — intento {2}: no se obtuvo imagen.", config.Codigo, codigoCamara, intento);
                    }
                    else
                    {
                        var (patente, certeza) = LlamarALPR(imagen, camara);
                        var estado = !string.IsNullOrEmpty(patente) ? "exitosas" : "fallidas";
                        var rutaImagen = IdentificacionVehicularHelper.GuardarImagen(log, imagen, contentType, config.Codigo, codigoCamara, estado);
                        if (!string.IsNullOrEmpty(patente))
                        {
                            log.Debug("[CIV:{0}] Cámara {1} — intento {2}: patente={3}", config.Codigo, codigoCamara, intento, patente);
                            detalle.Patente = patente;
                            detalle.Certeza = certeza;
                            detalle.RutaImagen = rutaImagen;
                            return detalle;
                        }

                        detalle.RutaImagen = rutaImagen;
                        log.Debug("[CIV:{0}] Cámara {1} — intento {2}: sin patente.", config.Codigo, codigoCamara, intento);
                    }
                }
                catch (Exception ex)
                {
                    detalle.Error = ex.Message;
                    log.Error(ex, "[CIV:{0}] Cámara {1} — intento {2}: error.", config.Codigo, codigoCamara, intento);
                }

                if (intento < maxReintentos && delayMs > 0)
                    await Task.Delay(delayMs);
            }

            return detalle;
        }

        private void GuardarImageHikVision(DetalleCamaraCIV detalle)
        {
            try
            {
                var (imagen, contentType) = ImagenHelper.ConvertirUrlAByte(detalle.RutaImagen, 5000);
                if (imagen == null)
                {
                    log.Warn("[CIV:{0}] Cámara {1} — no se pudo descargar imagen desde URL temporal.", config.Codigo, detalle.CodigoCamara);
                    return;
                }

                var estado = !string.IsNullOrEmpty(detalle.Patente) ? "exitosas" : "fallidas";
                var ruta = IdentificacionVehicularHelper.GuardarImagen(log, imagen, contentType, config.Codigo, detalle.CodigoCamara, estado);
                if (ruta != null)
                    detalle.RutaImagen = ruta;
            }
            catch (Exception ex)
            {
                log.Error(ex, "[CIV:{0}] Error al persistir imagen de URL temporal para cámara {1}.", config.Codigo, detalle.CodigoCamara);
            }
        }

        private (byte[] imagen, string contentType) ObtenerImagen(ConfigCamara camara)
        {
            NetworkCredential credentials = null;
            if (!string.IsNullOrEmpty(camara.NombreUsuario) && !string.IsNullOrEmpty(camara.Contrasenia))
                credentials = new NetworkCredential(camara.NombreUsuario, Encriptador.Decrypt(camara.Contrasenia));

            var (imagen, contentType) = ImagenHelper.ConvertirUrlAByte(camara.Uri, camara.TimeoutLectura, credentials);
            if (imagen == null)
                log.Warn("[CIV:{0}] Cámara {1} no devolvió una imagen válida.",
                    config.Codigo, camara.Dispositivo?.Codigo);

            return (imagen, contentType);
        }

        private (string patente, float? certeza) LlamarALPR(byte[] imagen, ConfigCamara camara)
        {
            var resultado = ALPRConnectionHelper.CreateChannel(channel =>
                channel.LeerPatente(
                    imagen,
                    camara.MargenIzquierdo ?? 0,
                    camara.MargenDerecho ?? 0,
                    camara.MargenSuperior ?? 0,
                    camara.MargenInferior ?? 0),
                log);

            if (resultado == null || string.IsNullOrEmpty(resultado.Patente))
                return (null, null);

            return (resultado.Patente, resultado.Confianza);
        }

        private bool ConsultarPresencia()
        {
            var sensor = driverPresencia;
            if (sensor == null)
                return false;

            try
            {
                log.Debug("[CIV:{0}] Consulta Estado Sensor Presencia.", config.Codigo);
                var resultado = sensor.ConsultaEstadoActual();
                return resultado?.EstadoActivo ?? false;
            }
            catch (Exception ex)
            {
                log.Warn(ex, "[CIV:{0}] Error al consultar sensor de presencia.", config.Codigo);
                return false;
            }
        }

        public (IList<EstadoDispositivoCIVDto> estado, IList<NotificacionCIVDto> notificaciones) ObtenerEstadoActual()
        {
            var estado = new List<EstadoDispositivoCIVDto>();

            if (config.ConfigLectorTarjetas != null)
                estado.Add(new EstadoDispositivoCIVDto
                {
                    CodigoDispositivo = config.ConfigLectorTarjetas.Dispositivo?.Codigo,
                    TipoDispositivo   = "LectorTarjetas",
                    Conectado         = driverTriggerLector != null,
                    UltimoValor       = ultimoValorTarjeta
                });

            if (config.ConfigSensorVehicular != null)
                estado.Add(new EstadoDispositivoCIVDto
                {
                    CodigoDispositivo = config.ConfigSensorVehicular.Dispositivo?.Codigo,
                    TipoDispositivo   = "SensorVehicular",
                    Conectado         = driverSensorVehicular != null
                });

            if (config.ConfigSensorPresencia != null)
                estado.Add(new EstadoDispositivoCIVDto
                {
                    CodigoDispositivo = config.ConfigSensorPresencia.Dispositivo?.Codigo,
                    TipoDispositivo   = "SensorPresencia",
                    Conectado         = driverPresencia != null,
                    VehiculoPresente  = ultimaPresencia
                });

            lock (detalleLock)
            {
                foreach (var camaraCIV in config.Camaras ?? new List<ConfigIdentificacionVehicularCamara>())
                {
                    var codigo = camaraCIV.ConfigCamara?.Dispositivo?.Codigo;
                    ultimoDetallePorCamara.TryGetValue(codigo ?? string.Empty, out var detalle);
                    estado.Add(new EstadoDispositivoCIVDto
                    {
                        CodigoDispositivo = codigo,
                        TipoDispositivo   = "Camara",
                        Conectado         = true,
                        UltimaPatente     = detalle?.Patente
                    });
                }
            }

            List<NotificacionCIVDto> notificaciones;
            lock (bufferLock)
                notificaciones = bufferNotificaciones.ToList();
            notificaciones.Reverse();

            return (estado, notificaciones);
        }

        public ConfigIdentificacionVehicular Config => config;

        public void RegistrarNotificacion(NotificacionCIVDto dto)
        {
            lock (bufferLock)
            {
                if (bufferNotificaciones.Count >= 50)
                    bufferNotificaciones.Dequeue();
                bufferNotificaciones.Enqueue(dto);
            }
        }

        public void ActualizarConfig(ConfigIdentificacionVehicular nuevaConfig)
        {
            config = nuevaConfig;
            log.Debug("[CIV:{0}] Configuración actualizada.", config.Codigo);
        }

        public void Dispose()
        {
            AsignarTriggerDriverLectorTarjeta(null);
            AsignarTriggerDriverSensor(null);
            driverPresencia = null;
            procesamientoSemaphore.Dispose();
            log.Debug("[CIV:{0}] Disposed.", config.Codigo);
        }
    }
}
