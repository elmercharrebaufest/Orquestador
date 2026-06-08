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
using System.Configuration;
using System.IO;
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

        private void OnEventoTriggerLecturaTarjeta(object sender, EventoDriverEventArgs args)
        {
            var notificacion = args.Notificacion;
            if (notificacion.CodigoEvento != CodigosEventos.LecturaTarjetaRecibida)
                return;

            string valorTarjeta = null;
            notificacion.Datos?.TryGetValue("Tarjeta", out valorTarjeta);
            log.Debug("[CIV:{0}] Trigger lector recibido - tarjeta:{1}.", config.Codigo, valorTarjeta);

            Task.Run(() => ProcesarTrigger(valorTarjeta, "Tarjeta"));
        }

        private void OnEventoTriggerSensorVehicular(object sender, EventoDriverEventArgs args)
        {
            if (args.Notificacion.CodigoEvento != CodigosEventos.EntradaActivada)
                return;

            log.Debug("[CIV:{0}] Trigger sensor vehicular recibido.", config.Codigo);
            Task.Run(() => ProcesarTrigger(null, "Sensor"));
        }

        private void ProcesarTrigger(string valorTarjeta, string trigger)
        {
            procesamientoSemaphore.Wait();
            try
            {
                var fechaEvento = DateTime.Now;
                var camaras = config.Camaras;
                var tareasCaptura = new List<Task<DetalleCamaraCIV>>();

                if (camaras != null)
                {
                    foreach (var camaraCIV in camaras)
                    {
                        var camara = camaraCIV.ConfigCamara;
                        tareasCaptura.Add(Task.Run(() => CapturarPatente(camara)));
                    }
                }

                var tareaPresencia = Task.Run(() => ConsultarPresencia());

                Task.WaitAll(tareasCaptura.ToArray());
                var estadoPresencia = tareaPresencia.Result;

                string patente = null;
                string error = null;
                var detalles = new List<DetalleCamaraCIV>();

                foreach (var t in tareasCaptura)
                {
                    var detalle = t.Result;
                    detalles.Add(detalle);
                    if (patente == null && !string.IsNullOrEmpty(detalle.Patente))
                        patente = detalle.Patente;
                    if (error == null && detalle.Error != null)
                        error = detalle.Error;
                }

                log.Debug("[CIV:{0}] Procesamiento completo — patente:{1} presencia:{2}", config.Codigo, patente, estadoPresencia);

                var datos = new Dictionary<string, string>
                {
                    ["Tarjeta"] = valorTarjeta ?? string.Empty,
                    ["Patente"] = patente ?? string.Empty,
                    ["FechaEvento"] = fechaEvento.ToString("o"),
                    ["VehiculoPresente"] = estadoPresencia.ToString(),
                    ["Error"] = error ?? string.Empty,
                    ["Detalle"] = detalles.ToJson(),
                    ["Trigger"] = trigger
                };

                notificar(new NotificacionEvento
                {
                    CodigoDispositivo = config.Codigo,
                    CodigoEvento = CodigosEventos.IdentificacionVehicular,
                    Datos = datos
                });

                // Actualizar snapshot
                ultimoValorTarjeta = valorTarjeta;
                ultimaPresencia = estadoPresencia;
                lock (detalleLock)
                {
                    foreach (var d in detalles)
                        ultimoDetallePorCamara[d.CodigoCamara] = d;
                }

                // Agregar al buffer circular (máx. 50)
                var dtoBuffer = new NotificacionCIVDto
                {
                    CodigoEvento      = CodigosEventos.IdentificacionVehicular,
                    CodigoDispositivo = config.Codigo,
                    Valor             = valorTarjeta ?? string.Empty,
                    VehiculoPresente  = estadoPresencia,
                    Patente           = patente,
                    FechaEvento       = fechaEvento,
                    Detalles          = detalles,
                    JsonCompleto      = datos.ToJson()
                };
                lock (bufferLock)
                {
                    if (bufferNotificaciones.Count >= 50)
                        bufferNotificaciones.Dequeue();
                    bufferNotificaciones.Enqueue(dtoBuffer);
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

        private DetalleCamaraCIV CapturarPatente(ConfigCamara camara)
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
                        var rutaImagen = GuardarImagen(imagen, contentType, codigoCamara, !string.IsNullOrEmpty(patente) ? "exitosas" : "fallidas");
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
                    Thread.Sleep(delayMs);
            }

            return detalle;
        }

        private (byte[] imagen, string contentType) ObtenerImagen(ConfigCamara camara)
        {
            var request = (HttpWebRequest)WebRequest.Create(camara.Uri);
            request.Timeout = camara.TimeoutLectura;

            if (!string.IsNullOrEmpty(camara.NombreUsuario) && !string.IsNullOrEmpty(camara.Contrasenia))
                request.Credentials = new NetworkCredential(camara.NombreUsuario, Encriptador.Decrypt(camara.Contrasenia));

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if ((response.StatusCode == HttpStatusCode.OK
                     || response.StatusCode == HttpStatusCode.Moved
                     || response.StatusCode == HttpStatusCode.Redirect)
                    && response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    using (var stream = response.GetResponseStream())
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return (ms.ToArray(), response.ContentType);
                    }
                }

                log.Warn("[CIV:{0}] Cámara {1} respondió con ContentType={2} StatusCode={3}.",
                    config.Codigo, camara.Dispositivo?.Codigo, response.ContentType, response.StatusCode);
                return (null, null);
            }
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

        private string GuardarImagen(byte[] imagen, string contentType, string codigoCamara, string estado)
        {
            var rutaFotos = ConfigurationManager.AppSettings["IdentificacionVehicular.RutaFotos"];
            if (string.IsNullOrEmpty(rutaFotos))
            {
                log.Warn("[CIV:{0}] No se configuró IdentificacionVehicular.RutaFotos. No se guardará la imagen.", config.Codigo);
                return null;
            }

            try
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var extension = contentType.Substring(contentType.IndexOf("/", StringComparison.Ordinal) + 1);

                var fullPath = Path.Combine(rutaFotos, fecha, config.Codigo, codigoCamara ?? "desconocida", estado);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, $"{fileName}.{extension}");
                File.WriteAllBytes(filePath, imagen);
                log.Debug("[CIV:{0}] Imagen guardada en {1}", config.Codigo, filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                log.Error(ex, "[CIV:{0}] Error al guardar imagen de cámara {1}.", config.Codigo, codigoCamara);
                return null;
            }
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
