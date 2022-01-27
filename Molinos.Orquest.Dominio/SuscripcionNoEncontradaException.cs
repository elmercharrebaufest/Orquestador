using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio
{
    [Serializable]
    public class SuscripcionNoEncontradaException : Exception
    {
        public SuscripcionNoEncontradaException()
        {
        }

        public SuscripcionNoEncontradaException(string message) : base(message)
        {
        }

        public SuscripcionNoEncontradaException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SuscripcionNoEncontradaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
