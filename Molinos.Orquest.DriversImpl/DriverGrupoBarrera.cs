using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverGrupoBarrera : DriverBase, IDriverGrupoBarrera
    {
        public override Type TipoDispositivo => throw new NotImplementedException();

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            throw new NotImplementedException();
        }

        public override void VerificarDispositivo()
        {
            throw new NotImplementedException();
        }
    }
}