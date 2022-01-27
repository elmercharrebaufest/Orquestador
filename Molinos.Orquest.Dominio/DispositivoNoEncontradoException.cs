using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio
{
    [Serializable]
    public class DispositivoNoEncontradoException : Exception
    {
        public DispositivoNoEncontradoException()
        {
        }

        public DispositivoNoEncontradoException(string message) : base(message)
        {
        }

        public DispositivoNoEncontradoException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DispositivoNoEncontradoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
