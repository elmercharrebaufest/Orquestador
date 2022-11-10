using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.ServicioALPR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;
using ResultadoObtenerPatente = Molinos.Orquest.Dominio.Resultados.ResultadoObtenerPatente;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorCamaraALPRDummy : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigSensor configSensor;
        private ConfigCamara configCamara;
        private IDriverItc driverItc;
        private readonly IServicioALPR servicioALPR;

        private readonly List<string> eventosSoportados = new List<string> {
             CodigosEventos.EntradaActivada
            , CodigosEventos.EntradaDesactivada
            , CodigosEventos.ErrorConexionDispositivo
            , CodigosEventos.ConexionDispositivoCorrecta
            , CodigosEventos.CambioEstadoSensor
            ,CodigosEventos.CambioEstadoSensorCamaraALPR };

        public DriverSensorCamaraALPRDummy(IServicioALPR servicioALPR)
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

            if (notificacion.Datos.ContainsKey("Mensaje"))
                estado = notificacion.Datos["Mensaje"];

            codigoEvento = CodigosEventos.CambioEstadoSensorCamaraALPR;
            if (estado == "True")
            {
                var resultadoALPR = TomarFoto();
                datos = new Dictionary<string, string>
                        {
                            { "Patente",resultadoALPR.Patente},
                            { "Estado",estado},
                        };
            }
            else
            {
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
        }

        private ResultadoObtenerPatente TomarFoto()
        {
            var resultado = new ResultadoObtenerPatente();
            var rutaFoto = ConfigurationManager.AppSettings["FotoRutaALPRDummy"];
            byte[] imagen = null;
            if (File.Exists(rutaFoto))
            {
                using (var m = new MemoryStream())
                {
                    Image image = Image.FromFile(rutaFoto);
                    image.Save(m, ImageFormat.Jpeg);
                    imagen = m.ToArray();
                }
            }
            else
            {
                Log.Debug($"Foto de ruta ALPR {rutaFoto} no válida");
            }

            if (imagen != null && imagen.Length > 0)
            {
                resultado = LlamarALPR(imagen);
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
            return resultadoObtenerPatente;
        }
    }
}