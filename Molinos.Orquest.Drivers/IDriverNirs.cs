using System;
using System.Collections.Generic;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverNirs : IDriver
    {
        Dictionary<string, decimal> ObtenerAnalisis(string codigoDeMaterial, string codigoDeMuestra);
    }
}
