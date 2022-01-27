using System;
using System.Collections.Generic;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.Servicios
{
    public interface IDriverFactory
    {
        TDriver Driver<TDriver>(Dispositivo dispositivo) where TDriver : class, IDriver;
        TDriver DriverLogico<TDriver>(Dispositivo dispositivo, IDriver driverFisico) where TDriver : class, IDriver;
        IList<string> DriversDisponibles<TDriver>() where TDriver : class, IDriver;
    }
}
