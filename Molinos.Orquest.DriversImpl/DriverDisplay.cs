using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
namespace Molinos.Orquest.DriversImpl
{
    public class DriverDisplay : DriverBase, IDriverDisplay, IDriverLogico
    {
        private IDriverItc driverDisplay;
        private ConfigDisplay configDisplay;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigDisplay); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configDisplay = (ConfigDisplay)configuracion;
        }

        public override void VerificarDispositivo()
        {
            driverDisplay.VerificarDispositivo();
        }

        public void Abrir()
        {
            driverDisplay.ActivarSalida(configDisplay.NumeroSalida, configDisplay.Url, "0");
        }

        public IDriver DriverFisico
        {
            set { driverDisplay = (IDriverItc)value; }
        }
    }
}
