using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Drivers
{
    [Serializable]
    public class LecturaEscrituraDriverException : DriverException
    {
        protected LecturaEscrituraDriverException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public LecturaEscrituraDriverException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public LecturaEscrituraDriverException(string message)
            : base(message)
        {
        }

        public LecturaEscrituraDriverException()
        {
        }
    }
}
