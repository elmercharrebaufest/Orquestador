using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBarreraItc : DriverBase, IDriverBarrera, IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigBarrera configBarrera;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigBarrera); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configBarrera = (ConfigBarrera)configuracion;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public void Abrir()
        {
            Log.Info("Activando Salida: Barrera={0}", configBarrera.Dispositivo.Codigo);
            if (configBarrera.Sensor != null)
            {

                DateTime endDate = DateTime.Now.AddMinutes(configBarrera.TiempoMaximoEsperaReintento.Value);
                bool abrio = false;
                while (DateTime.Now <= endDate)
                {
                    if (driverItc.ConsultarEstadoEntrada(configBarrera.Sensor.NumeroEntrada))
                    {
                        driverItc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
                        abrio = true;
                        break;
                    }
                    Thread.Sleep(configBarrera.TiempoEsperaReintento.Value * 1000);
                }
                if (!abrio)
                    throw new DriverException(string.Format("No se pudo abrir la Barrera {0}", configBarrera.Dispositivo.Codigo));

            }
            else
            {
                driverItc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
            }

        }
      
        public void Cerrar()
        {
            driverItc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 0 : 1).ToString(), configBarrera.TiempoActivacion.ToString());
        }

        public void AbrirMaestro()
        {
            //Log.Info("Abriendo Salida Maestro: Barrera={0}", configBarrera.Dispositivo.Codigo);
            Log.Debug("Salida: Barrera={0} NumeroSalida={1}", configBarrera.Dispositivo.Codigo, configBarrera.NumeroSalida);
            driverItc.ActivarSalida(configBarrera.NumeroSalida, (configBarrera.EstadoAbierta ? 1 : 0).ToString(), configBarrera.TiempoActivacion.ToString());
        }

        public IDriver DriverFisico
        {
            set { driverItc = (IDriverItc)value; }
        }

    }
}
