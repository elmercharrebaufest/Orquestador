using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCabezalIPDummySinCero : DriverBase, IDriverCabezal
    {

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCabezal); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
        }

        public override void VerificarDispositivo()
        {
        }

        public decimal? ObtenerPeso()
        {
            Thread.Sleep(100);
            var peso = (DateTime.Now.Millisecond*10)+1;
            return peso;
        }

        public bool ForzarCero()
        {
            return true;
        }
    }
}
