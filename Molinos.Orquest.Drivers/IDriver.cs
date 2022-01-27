using System;
using System.Collections.Generic;
using Molinos.Orquest.Dominio.Entidades;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Drivers
{
    public interface IDriver
    {
        event EventHandler<EventoDriverEventArgs> EventoDriver;
        Type TipoDispositivo { get; }
        IEnumerable<string> EventosSoportados { get; }
        void Inicializar(string codigo, ConfigDispositivo configuracion);
        void VerificarDispositivo();
        void InformarEstado();
        ILogger Log { get; set; }
        bool MantenerConectado();
    }
}
