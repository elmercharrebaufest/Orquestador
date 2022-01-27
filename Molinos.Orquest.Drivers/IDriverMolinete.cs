using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverMolinete : IDriver
    {
        void HabilitarTransito(string direccion, string tarjeta,int? fichadaId);
    }
}
