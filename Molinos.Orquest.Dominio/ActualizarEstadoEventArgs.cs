using System;

namespace Molinos.Orquest.Dominio
{
    public class ActualizarEstadoEventArgs : EventArgs
    {
        public DateTime Vencimiento { get; set; }
    }
}
