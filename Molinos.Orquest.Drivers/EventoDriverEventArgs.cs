using System;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Drivers
{
    public class EventoDriverEventArgs : EventArgs
    {
        public NotificacionEvento Notificacion { get; set; }
    }
}
