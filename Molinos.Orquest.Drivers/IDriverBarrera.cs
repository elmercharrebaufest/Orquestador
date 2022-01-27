using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverBarrera : IDriver
    {
        void Abrir();
        void Cerrar();
        void AbrirMaestro();
    }
}
