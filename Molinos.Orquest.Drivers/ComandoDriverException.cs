using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Drivers
{
    [Serializable]
    public class ComandoDriverException : DriverException
    {
        public ComandoDriverException()
        {
        }

        public ComandoDriverException(string message) : base(message)
        {
        }

        public ComandoDriverException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ComandoDriverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
