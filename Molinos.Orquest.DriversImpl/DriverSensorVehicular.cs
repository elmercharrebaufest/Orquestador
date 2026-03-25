using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
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
                var (patente, error) = TomarFoto();
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

        private (string patente, string error) TomarFoto()
        {
            string patente = string.Empty;
            string error = null;

            try
            {
                var request = (HttpWebRequest)WebRequest.Create(configCamara.Uri);
                request.Timeout = configCamara.TimeoutLectura;

                if (!string.IsNullOrEmpty(configCamara.NombreUsuario) && !string.IsNullOrEmpty(configCamara.Contrasenia))
                {
                    request.Credentials = new NetworkCredential(configCamara.NombreUsuario, Encriptador.Decrypt(configCamara.Contrasenia));
                }

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

        private (string patente, string error) LlamarALPR(byte[] imagen)
        {
            string patente = string.Empty;
            string error = null;

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
