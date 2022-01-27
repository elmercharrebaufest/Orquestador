using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Repositorio
{
    [Serializable]
    public class EntidadReferenciadaException : Exception
    {
        public EntidadReferenciadaException()
        {
        }

        public EntidadReferenciadaException(string message)
            : base(message)
        {
        }

        public EntidadReferenciadaException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected EntidadReferenciadaException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
