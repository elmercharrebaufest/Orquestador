using Molinos.Orquest.Dominio.Dtos;
using System;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverHumedimetro : IDriver
    {
        decimal? ObtenerHumedad(DateTime? fechaDeInicio = null);

        HumedimetroResultadoDto ObtenerHumedadPH(DateTime? fechaDeInicio = null);
    }
}