using System;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    [Serializable]
    public class ProcesadorException : Exception
    {
        public ProcesadorException(string message) : base(message)
        {
        }

        public ProcesadorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
