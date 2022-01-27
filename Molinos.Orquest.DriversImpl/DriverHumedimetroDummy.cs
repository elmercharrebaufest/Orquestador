using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroDummy : DriverBase, IDriverHumedimetro
    {
        public override Type TipoDispositivo
        {
            get { return typeof (ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
        }

        public override void VerificarDispositivo()
        {
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            Thread.Sleep(100);
            return DateTime.Now.Millisecond % 100;
        }
    }
}
