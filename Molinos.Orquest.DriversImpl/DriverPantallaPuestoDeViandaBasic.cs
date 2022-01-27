using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverPantallaPuestoDeViandaBasic : DriverBase, IDriverPantallaPuestoDeVianda
    {
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigPantallaPuestoDeVianda); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
        }

        public override void VerificarDispositivo()
        {
        }
    }
}
