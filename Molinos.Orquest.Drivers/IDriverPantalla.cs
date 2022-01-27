using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Resultados;
using System.Collections.Generic;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverPantalla : IDriver
    {
        ResultadoEjecutar ObtenerUltimaFoto();
    }
}
