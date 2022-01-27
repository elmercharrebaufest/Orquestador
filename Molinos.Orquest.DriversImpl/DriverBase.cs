using System;
using System.Collections.Generic;
using System.Linq;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public abstract class DriverBase : IDriver, IDisposable
    {
        public event EventHandler<EventoDriverEventArgs> EventoDriver;
        public abstract Type TipoDispositivo { get; }

        public virtual void InformarEstado() { }

        public ILogger Log { get; set; }
        public virtual bool MantenerConectado()
        {
            return false;
        }

        public virtual IEnumerable<string> EventosSoportados
        {
            // Por defecto no hay eventos...
            get { return Enumerable.Empty<string>(); }
        }

        public abstract void Inicializar(string codigo, ConfigDispositivo configuracion);

        public abstract void VerificarDispositivo();

        protected virtual void OnEventoDriver(EventoDriverEventArgs e)
        {
            EventHandler<EventoDriverEventArgs> handler = EventoDriver;
            if (handler != null)
            {
                handler(this, e);
            }

        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
