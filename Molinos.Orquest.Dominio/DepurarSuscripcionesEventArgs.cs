using System;

namespace Molinos.Orquest.Dominio
{
    public class DepurarSuscripcionesEventArgs : EventArgs
    {
        public DateTime Vencimiento { get; set; }
    }
}
