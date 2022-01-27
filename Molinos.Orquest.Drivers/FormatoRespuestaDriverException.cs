using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Drivers
{
    [Serializable]
    public class FormatoRespuestaDriverException : DriverException
    {
        protected FormatoRespuestaDriverException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public FormatoRespuestaDriverException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public FormatoRespuestaDriverException(string message) : base(message)
        {
        }

        public FormatoRespuestaDriverException()
        {
        }
    }
}
