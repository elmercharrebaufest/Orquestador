using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;


namespace Molinos.Orquest.DriversImpl
{
   public class DriverJsonToIotBox : DriverBase, IDriverJsonToIotBox,IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigJsonToIotBox configJsontoIotBox;
        

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigJsonToIotBox); }
        }

        public IDriver DriverFisico { set => throw new NotImplementedException(); }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configJsontoIotBox = (ConfigJsonToIotBox)configuracion;
        }

        public override void VerificarDispositivo()
        {
            
        }
        public void EnviarJson(string Json)
        {
            //var formatoJson = configJsontoIotBox.Json;
            //var desJson = JsonConvert.DeserializeObject<PersonasHabilitadas>(Json);
            
            driverItc.ActivarSalida(configJsontoIotBox.NumeroSalida, Json, "");
        }

        public void Cerrar()
        {
            throw new NotImplementedException();
        }

        public void EnviarPersonaHabilitada(int puesto)
        {
            throw new NotImplementedException();

        }
    }
}
