using Molinos.Orquest.Dominio.Dtos;
using System.Collections.Generic;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverMeteorologica : IDriver
    {
        List<MeteorologicaDto> ObtenerGraficosMeteorologicos();
        decimal ObtenerDireccionViento();


    }
}
