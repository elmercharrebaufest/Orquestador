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
using System.Globalization;
using System.IO;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;
using ResultadoObtenerPatente = Molinos.Orquest.Dominio.Resultados.ResultadoObtenerPatente;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorCamaraALPRDummy : DriverBase, IDriverSensor, IDriverLogico
    {
        private readonly IServicioALPR servicioALPR;
        private IDriverItc driverItc;

        private string codigoDispositivo;
        private string codigoEventoITC;
        private string entrada;

        private ConfigSensor configSensor;
        private ConfigCamara configCamara;

        private readonly List<string> eventosSoportados = new List<string> {
             CodigosEventos.EntradaActivada
            , CodigosEventos.EntradaDesactivada
            , CodigosEventos.ErrorConexionDispositivo
            , CodigosEventos.ConexionDispositivoCorrecta
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
            Log.Info("DriverSensorCamaraALPRDummy evento {0}", evento.ToJson());

            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {
                var estado = string.Empty;

                if (notificacion.Datos.ContainsKey("Dato"))
                    estado = notificacion.Datos["Dato"];

                if (estado.ToUpper() == "TRUE")
                {
                    codigoEventoITC = CodigosEventos.EntradaActivada;
                    var resultadoALPR = TomarFoto();
                    notificacion.Datos["Patente"] = resultadoALPR.Patente;
                    notificacion.Datos["Estado"] = estado;

                    var eventoNotification = new EventoDriverEventArgs
                    {
                        Notificacion = new NotificacionEvento
                        {
                            CodigoDispositivo = codigoDispositivo,
                            CodigoEvento = CodigosEventos.CambioEstadoSensorCamaraALPR,
                            Datos = notificacion.Datos
                        }
                    };
                    Log.Info("DriverSensorCamaraALPRDummy EventoNotification {0}", eventoNotification.ToJson());
                    OnEventoDriver(eventoNotification);
                }
                else
                {
                    codigoEventoITC = CodigosEventos.EntradaDesactivada;
                }
                NotificarEventoITC(notificacion.Datos);
            }
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
            resultadoObtenerPatente.Patente = resultadoALPR.Patente;
            return resultadoObtenerPatente;
        }

        private void NotificarEventoITC(Dictionary<string, string> datos)
        {
            var eventoNotification = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = codigoEventoITC,
                    Datos = datos
                }
            };

            OnEventoDriver(eventoNotification);
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            Log.Debug($"Es Evento Para Dispositivo: {notificacion.CodigoEvento} Datos: {notificacion.Datos}");
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }
    }
}