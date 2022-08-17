using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBarreraGrupo : DriverBase, IDriverBarrera
    {
        public override Type TipoDispositivo => throw new NotImplementedException();

        public void Abrir()
        {
            throw new NotImplementedException();
        }

        public void AbrirMaestro()
        {
            throw new NotImplementedException();
        }

        public void Cerrar()
        {
            throw new NotImplementedException();
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
        }

        public override void VerificarDispositivo()
        {
            throw new NotImplementedException();
        }
    }
}