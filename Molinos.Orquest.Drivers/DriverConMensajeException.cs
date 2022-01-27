using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Drivers
{
    [Serializable]
    public class DriverConMensajeException : DriverException
    {
        public DriverConMensajeException()
        {
        }

        public DriverConMensajeException(string message) : base(message)
        {
        }

        public DriverConMensajeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DriverConMensajeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
