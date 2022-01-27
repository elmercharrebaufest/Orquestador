using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBarreraPlc : DriverBase, IDriverBarrera, IDriverLogico
    {
        private string codigoBarrera;

        private IDriverItc driverPlc;
        private bool ejecutandoApertura;
        private ConfigBarrera configBarrera;
        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigBarrera); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configBarrera = (ConfigBarrera)configuracion;
            codigoBarrera = configBarrera.Dispositivo.Codigo;
            ejecutandoApertura = false;

        }

        public override void VerificarDispositivo()
        {
            driverPlc.VerificarDispositivo();
        }

        public void Abrir()
        {
            if (configBarrera.Sensor != null)
            {
                Log.Info("Abriendo Salida con sensor: Barrera={0}", configBarrera.Dispositivo.Codigo);
                DateTime endDate = DateTime.Now.AddMinutes(configBarrera.TiempoMaximoEsperaReintento.Value);
                if (ejecutandoApertura)
                {
                    throw new DriverException(string.Format("Ya se esta ejecutando la apertura {0}", configBarrera.Dispositivo.Codigo));
                }
                Task.Run(() =>
                {
                    ejecutandoApertura = true;
                    while (ejecutandoApertura)
                    {
                        if (driverPlc.ConsultarEstadoEntrada(configBarrera.Sensor.NumeroEntrada))
                        {
                            Log.Info("Salida: Barrera={0} NumeroSalida={1}", configBarrera.Dispositivo.Codigo, configBarrera.NumeroSalida);
                            driverPlc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
                            ejecutandoApertura = false;
                            break;
                        }
                        if(DateTime.Now > endDate)
                        {
                            NotificarEstado(CodigosEventos.ErrorConexionDispositivo);
                        }
                        Thread.Sleep(configBarrera.TiempoEsperaReintento.Value * 1000);
                    }
                    finCiclo.Set();
                });

            }
            else
            {
                Log.Info("Abriendo Salida sin sensor: Barrera={0}", configBarrera.Dispositivo.Codigo);
                Log.Info("Salida: Barrera={0} NumeroSalida={1}", configBarrera.Dispositivo.Codigo, configBarrera.NumeroSalida);
                driverPlc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
            }

        }
        public void Cerrar()
        {
            Log.Info("Cerrando Salida sin sensor: Barrera={0}", configBarrera.Dispositivo.Codigo);
            Log.Info("Salida: Barrera={0} NumeroSalida={1}", configBarrera.Dispositivo.Codigo, configBarrera.NumeroSalida);
            driverPlc.DesactivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());

        }
        public IDriver DriverFisico
        {
            set { driverPlc = (IDriverItc)value; }
        }
        private void NotificarEstado(string codigoEvento, Exception e = null)
        {
            try
            {
                
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = configBarrera.Dispositivo.Codigo,
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
                Log.Error(ex, "Barrera {0}: No se pudo notificar el evento {1}", codigoBarrera, codigoEvento);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (ejecutandoApertura)
                {
                    ejecutandoApertura = false;
                    finCiclo.WaitOne();
                    finCiclo.Dispose();
                }                
            }
        }
        public void AbrirMaestro()
        {
            Log.Info("Abriendo Salida Maestro: Barrera={0}", configBarrera.Dispositivo.Codigo);
            Log.Debug("Salida: Barrera={0} NumeroSalida={1}", configBarrera.Dispositivo.Codigo, configBarrera.NumeroSalida);
            driverPlc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
        }
    }
}
