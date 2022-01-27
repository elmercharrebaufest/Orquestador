using Molinos.Orquest.Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverTag : IDriver
    {
        ResultadoEjecutarQuery EjecutarQuery();
        ResultadoEjecutarQuery EjecutarQuery(string query);
    }
}
