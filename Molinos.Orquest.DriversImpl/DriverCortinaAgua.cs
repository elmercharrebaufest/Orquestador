using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using AngleSharp.Html.Dom;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Resultados;
using System.Configuration;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCortinaAgua : DriverBase, IDriverCortinaAgua, IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigCortinaAgua configCortinaAgua;
        private DriverMeteorologica driverMeteorologica;
        private bool notificaEventos = false;
        private bool? falloUltimaConexion;
        private string codigoDispositivo;
        private Exception errorUltimaConexion;
        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);
        decimal direccionViento;
        private DateTime fechaUltimaActivacion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.EntradaActivada, CodigosEventos.EntradaDesactivada, CodigosEventos.ErrorConexionDispositivo, CodigosEventos.ConexionDispositivoCorrecta };

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCortinaAgua); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            
            codigoDispositivo = codigo;
            configCortinaAgua = (ConfigCortinaAgua)configuracion;
                      
            driverMeteorologica = new DriverMeteorologica();
            driverMeteorologica.Inicializar(configCortinaAgua.Estacion.Dispositivo.Codigo, configCortinaAgua.Estacion);

            notificaEventos = true;
            Task.Run(() =>
            {
                while (notificaEventos)
                {
                    try
                    {
                        ConsultarEstado();
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
                        {
                            Log.Debug($"Conexion reestablecida con la Cortina de Agua {codigoDispositivo}");
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, "Error al ConsultarEstado de la Cortina de Agua {0}", codigoDispositivo);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de la Cortina de Agua = {0}", codigoDispositivo);
                            NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }
                    }
                    Thread.Sleep(configCortinaAgua.IntervaloPooling);
                }
                finCiclo.Set();
            });
        }

        public override bool MantenerConectado()
        {
            return true;
        }


        private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
        {
            try
            {
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = codigoEvento,
                    Datos = e != null ? new Dictionary<string, string>
                            {
                                {"Error", e.Message},
                                {"Detalle", e.StackTrace}
                            } : new Dictionary<string, string>()
                };
                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Molinete {0}: No se pudo notificar el evento {1}", codigoDispositivo, codigoEvento);
            }
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
            
        }

        public void Abrir()
        {
            Log.Info("Activando Salida: Cortina de Agua={0}", configCortinaAgua.Dispositivo.Codigo);
        
            driverItc.ActivarSalida(configCortinaAgua.NumeroSalida, (configCortinaAgua.EstadoAbierta ? 1 : 0).ToString(), configCortinaAgua.TiempoActivacion.ToString());
            fechaUltimaActivacion = DateTime.Now;
            var nuevoEvento = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.EntradaActivada,
                    Datos =new Dictionary<string, string>(),
                }
            };
            OnEventoDriver(nuevoEvento);
        }

        public IDriver DriverFisico
        {
            set { driverItc = (IDriverItc)value; }
        }

        public void ConsultarEstado()
        {
            var fecha = fechaUltimaActivacion.AddMinutes(configCortinaAgua.TiempoActivacion ?? 0).AddMinutes(configCortinaAgua.TiempoDeEsperaActivacion ?? 0);
            direccionViento = driverMeteorologica.ObtenerDireccionViento();
            if (fecha < DateTime.Now  && 
                (direccionViento >= configCortinaAgua.DireccionDelVientoDesde &&
                direccionViento <= configCortinaAgua.DireccionDelVientoHasta))
            {
                Abrir();
            }
        }

        protected override void Dispose(bool disposing)
        {

        }
    }
}
