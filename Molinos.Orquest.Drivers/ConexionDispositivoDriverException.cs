using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Drivers
{
    [Serializable]
    public class ConexionDispositivoDriverException : DriverException
    {
        public ConexionDispositivoDriverException()
        {
        }

        public ConexionDispositivoDriverException(string message) : base(message)
        {
        }

        public ConexionDispositivoDriverException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConexionDispositivoDriverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
