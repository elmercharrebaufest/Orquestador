using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorVehicular : DriverBase, IDriverSensor, IDriverLogico
    {
        private IDriverItc driverItc;

        private string codigoDispositivo;
        private string entrada;

        private ConfigSensor configSensor;
        private ConfigCamara configCamara;

        private readonly List<string> eventosSoportados = new List<string> {
             CodigosEventos.EntradaActivada,
             CodigosEventos.VehiculoDetectado,
        };

        public DriverSensorVehicular()
        {
        }

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigSensor); }
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configSensor = (ConfigSensor)configuracion;
            configCamara = configSensor.Camara;
            entrada = configSensor.NumeroEntrada.ToString(CultureInfo.InvariantCulture);
        }

        public void NotificarEstadoActualSensor()
        {
            driverItc.NotificarEstadoActual(configSensor.NumeroEntrada);
        }

        public ResultadoEstadoSensor ConsultaEstadoActual()
        {
            Log.Info($"ConsultaEstadoActual Dispositivo : {codigoDispositivo}, Numero Entrada : {configSensor.NumeroEntrada}");

            return new ResultadoEstadoSensor
            {
                CodigoDispositivoSensor = codigoDispositivo,
                EstadoActivo = driverItc.ConsultarEstadoActual(configSensor.NumeroEntrada),
                Mensaje = Mensaje.ResultadoOK()
            };
        }

        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {
            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {
                var (patente, error) = TomarFotoConReintento();
                notificacion.Datos["Patente"] = patente;
                notificacion.Datos["Error"] = error;

                var eventoNotification = new EventoDriverEventArgs
                {
                    Notificacion = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = CodigosEventos.VehiculoDetectado,
                        Datos = notificacion.Datos
                    }
                };
                Log.Debug("DriverSensorVehicular Notificacion {0}", eventoNotification.ToJson());
                OnEventoDriver(eventoNotification);
            }
        }

        private (string patente, string error) TomarFotoConReintento()
        {
            var delayAntesTomarFotoMs = ObtenerConfiguracion("SensorVehicular.DelayAntesTomarFotoMs");
            var maxReintentos = ObtenerConfiguracion("SensorVehicular.MaxReintentosFotos");

            string patente = string.Empty;
            string error = string.Empty;

            for (int intento = 0; intento <= maxReintentos; intento++)
            {
                (patente, error) = TomarFoto();
                Log.Debug($"Intento {intento} de tomar foto patente: {patente} - error: {error}");

                if (!string.IsNullOrEmpty(patente))
                    return (patente, error);

                if (delayAntesTomarFotoMs > 0) 
                    Thread.Sleep(delayAntesTomarFotoMs);
            }

            return (patente, error);
        }

        private int ObtenerConfiguracion(string key)
        {
            var valor = ConfigurationManager.AppSettings[key];
            return (string.IsNullOrEmpty(valor) || !int.TryParse(valor, out int resultado)) 
                ? 0 
                : resultado;
        }

        private (string patente, string error) TomarFoto()
        {
            string patente = string.Empty;
            string error = string.Empty;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(configCamara.Uri);
                request.Timeout = configCamara.TimeoutLectura;

                if (!string.IsNullOrEmpty(configCamara.NombreUsuario) && !string.IsNullOrEmpty(configCamara.Contrasenia))
                    request.Credentials = new NetworkCredential(configCamara.NombreUsuario, Encriptador.Decrypt(configCamara.Contrasenia));

                var response = (HttpWebResponse)request.GetResponse();

                if ((response.StatusCode == HttpStatusCode.OK
                    || response.StatusCode == HttpStatusCode.Moved
                    || response.StatusCode == HttpStatusCode.Redirect) &&
                    response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                {
                    using (var inputStream = response.GetResponseStream())
                    {
                        var imagenAEscanear = LeerImagen(inputStream);
                        (patente, error) = LlamarALPR(imagenAEscanear);

                        var guardarFotosFallidas = ConfigurationManager.AppSettings["SensorVehicular.GuardarFotosFallidas"];
                        if (!string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(guardarFotosFallidas) && bool.TryParse(guardarFotosFallidas, out bool guardarFallidas) && guardarFallidas)
                            GuardarImagen(imagenAEscanear, response.ContentType, "fallidas");

                        var guardarFotosExitosas = ConfigurationManager.AppSettings["SensorVehicular.GuardarFotosExitosas"];
                        if (!string.IsNullOrEmpty(patente) && !string.IsNullOrEmpty(guardarFotosExitosas) && bool.TryParse(guardarFotosExitosas, out bool guardarExitosas) && guardarExitosas)
                            GuardarImagen(imagenAEscanear, response.ContentType, "exitosas");
                    }
                }
                else
                {
                    error = "Error en formato de respuesta de cámara";
                    Log.Error($"Formato de respuesta del dispositivo {configCamara.Dispositivo.Codigo} incorrecto para la url {configCamara.Uri}");
                }
            }
            catch (Exception ex)
            {
                error = "Error al obtener imagen de cámara";
                Log.Error($"Error al obtener patente: {ex.Message}");
            }

            return (patente, error);
        }

        private void GuardarImagen(byte[] imagen, string contentType, string estado)
        {
            var rutaFotos = ConfigurationManager.AppSettings["SensorVehicular.RutaFotos"];
            if (string.IsNullOrEmpty(rutaFotos))
            {
                Log.Warn("No se configuró SensorVehicular.RutaFotos. No se guardará la imagen fallida.");
                return;
            }

            try
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss_fff}";
                var extension = contentType.Substring(contentType.IndexOf("/", StringComparison.Ordinal) + 1);

                var fullPath = Path.Combine(rutaFotos, fecha, codigoDispositivo, estado);
                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, $"{fileName}.{extension}");
                File.WriteAllBytes(filePath, imagen);
                Log.Debug($"Imagen fallida guardada en {filePath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error al guardar imagen fallida del sensor vehicular: {ex.Message}");
            }
        }

        private (string patente, string error) LlamarALPR(byte[] imagen)
        {
            string patente = string.Empty;
            string error = string.Empty;

            try
            {
                var resultadoALPR = ALPRConnectionHelper.CreateChannel(channel =>
                {
                    return channel.LeerPatente(imagen, configCamara.MargenIzquierdo ?? 0, configCamara.MargenDerecho ?? 0, configCamara.MargenSuperior ?? 0, configCamara.MargenInferior ?? 0);
                }, Log);

                patente = resultadoALPR.Patente;
                
                if (string.IsNullOrEmpty(patente))
                    error = "No se pudo detectar la patente";
            }
            catch (Exception ex)
            {
                error = "Error al procesar imagen con ALPR";
                Log.Error($"Error en ALPR: {ex.Message}");
            }

            return (patente, error);
        }

        private byte[] LeerImagen(Stream inputStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && notificacion.Datos["Entrada"] == entrada;
        }
    }
}
