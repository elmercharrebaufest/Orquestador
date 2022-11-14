using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.ServicioALPR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;
using ResultadoObtenerPatente = Molinos.Orquest.Dominio.Resultados.ResultadoObtenerPatente;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorCamaraALPR : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigSensor configSensor;
        private ConfigCamara configCamara;
        private IDriverItc driverItc;
        private readonly IServicioALPR servicioALPR;
        private string codigoEventoITC;

        private readonly List<string> eventosSoportados = new List<string> {
             CodigosEventos.EntradaActivada
            , CodigosEventos.EntradaDesactivada
            , CodigosEventos.ErrorConexionDispositivo
            , CodigosEventos.ConexionDispositivoCorrecta
            , CodigosEventos.CambioEstadoSensor
            ,CodigosEventos.CambioEstadoSensorCamaraALPR };

        public DriverSensorCamaraALPR(IServicioALPR servicioALPR)
        {
            this.servicioALPR = servicioALPR;
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
            var datos = new Dictionary<string, string>();
            var codigoEvento = string.Empty;
            var estado = string.Empty;

            if (notificacion.Datos.ContainsKey("Dato"))
                estado = notificacion.Datos["Dato"];

            codigoEvento = CodigosEventos.CambioEstadoSensorCamaraALPR;
            if (estado == "True")
            {
                codigoEventoITC = CodigosEventos.EntradaActivada;
                var resultadoALPR = TomarFoto();
                datos = new Dictionary<string, string>
                        {
                            { "Patente",resultadoALPR.Patente},
                            { "Estado",estado},
                        };
            }
            else
            {
                codigoEventoITC = CodigosEventos.EntradaDesactivada;
                datos = new Dictionary<string, string>
                        {
                            { "Patente",string.Empty},
                            { "Estado",estado},
                        };
            }

            var eventoNotification = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = codigoEvento,
                    Datos = datos
                }
            };
            Log.Debug("DriverSensorCamaraALPRDummy Notificacion {0}", eventoNotification.ToJson());
            OnEventoDriver(eventoNotification);
            NotificarEventoITC(eventoNotification);
        }

        private ResultadoObtenerPatente TomarFoto()
        {
            var resultado = new ResultadoObtenerPatente();

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
                    resultado = LlamarALPR(imagenAEscanear);
                }
            }
            else
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para la url {1}", configCamara.Dispositivo.Codigo, configCamara.Uri));
            }
            return resultado;
        }

        private ResultadoObtenerPatente LlamarALPR(byte[] imagen)
        {
            var resultadoObtenerPatente = new ResultadoObtenerPatente();
            var resultadoALPR = servicioALPR.LeerPatente(imagen, configCamara.MargenIzquierdo ?? 0, configCamara.MargenDerecho ?? 0, configCamara.MargenSuperior ?? 0, configCamara.MargenInferior ?? 0);
            resultadoObtenerPatente.Imagen = resultadoALPR.Imagen;
            resultadoObtenerPatente.Confianza = resultadoALPR.Confianza;
            resultadoObtenerPatente.Patente = resultadoALPR.Patente;
            return resultadoObtenerPatente;
        }

        private byte[] LeerImagen(Stream inputStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void NotificarEventoITC(EventoDriverEventArgs eventoNotification)
        {
            eventoNotification.Notificacion.CodigoEvento = codigoEventoITC;
            OnEventoDriver(eventoNotification);
        }
    }
}