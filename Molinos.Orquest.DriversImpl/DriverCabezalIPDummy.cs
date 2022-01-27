using System;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCabezalIPDummy : DriverBase, IDriverCabezal
    {

        public int[] pesos;
        public int count;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCabezal); }
        }
        public DriverCabezalIPDummy()
        {
            pesos = new int[] { 10000, 9999, 5000, 250, 0 };
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
            int peso;
            var rnd = new Random();
            peso = pesos[rnd.Next(0,5)];

            return peso;
        }

        public bool ForzarCero()
        {
            return true;
        }
    }
}
