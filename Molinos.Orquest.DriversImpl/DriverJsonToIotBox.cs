using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject.Extensions.Logging;


namespace Molinos.Orquest.DriversImpl
{
   public class DriverJsonToIotBox : DriverBase, IDriverJsonToIotBox,IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigJsonToIotBox configJsontoIotBox;
        private readonly ILogger log;

        public DriverJsonToIotBox(ILogger log)
        {
            this.log = log;
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigJsonToIotBox); }
        }

        public IDriver DriverFisico { set => driverItc = (IDriverItc)value; }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configJsontoIotBox = (ConfigJsonToIotBox)configuracion;
            log.Info("Codigo dispositivo: " 
                + codigo + " Numero de salida: " 
                + configJsontoIotBox.NumeroSalida.ToString());
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }
        public void EnviarJson(string Json)
        {
            if (!ExtensionesSerializacion.IsValidJson(Json))
            {
                log.Error("El comando 'EjecutarEnviarJson' espera un texto en formato json");
                throw new DriverConMensajeException("El comando 'EjecutarEnviarJson' espera un texto en formato json");
            }
            try
            {
                driverItc.ActivarSalida(configJsontoIotBox.NumeroSalida, Json, "0");
                log.Info("Json: "
                    + Json + " Numero de salida: "
                    + configJsontoIotBox.NumeroSalida.ToString());
            }
            catch (Exception e)
            {
                log.Error("Error al activar la salida - se realizará un reintento", e);
                Thread.Sleep(1000);
                try
                {
                    driverItc.ActivarSalida(configJsontoIotBox.NumeroSalida, Json, "0");
                    log.Info("Json: "
                        + Json + " Numero de salida: "
                        + configJsontoIotBox.NumeroSalida.ToString());
                }
                catch (Exception ex)
                {
                    log.Error("Error al activar la salida - falló el reintento", ex);
                    throw new DriverConMensajeException("Error al activar la salida después del reintento", ex);
                }
            }
        }
    }
}
