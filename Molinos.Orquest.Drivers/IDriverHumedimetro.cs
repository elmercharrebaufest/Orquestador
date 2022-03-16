using System;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverHumedimetro : IDriver
    {
        decimal? ObtenerHumedad(DateTime? fechaDeInicio = null);

        decimal? ObtenerPH(DateTime? fechaDeInicio = null);
    }
}
