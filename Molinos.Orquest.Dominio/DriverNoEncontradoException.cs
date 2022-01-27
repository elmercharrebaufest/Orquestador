using System;
using System.Runtime.Serialization;
using Molinos.Orquest.Dominio.Entidades;

namespace Molinos.Orquest.Dominio
{
    [Serializable]
    public class DriverNoEncontradoException : Exception
    {
        public string CodigoDispositivo { get; private set; }
        public string ClaseDriver { get; private set; }

        public DriverNoEncontradoException(string message, string codigoDispositivo, string claseDriver) : base(message)
        {
            CodigoDispositivo = codigoDispositivo;
            ClaseDriver = claseDriver;
        }

        public DriverNoEncontradoException(string message, Exception innerException, string codigoDispositivo, string claseDriver) : base(message, innerException)
        {
            CodigoDispositivo = codigoDispositivo;
            ClaseDriver = claseDriver;
        }

        protected DriverNoEncontradoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CodigoDispositivo = info.GetString("CodigoDispositivo");
            ClaseDriver = info.GetString("ClaseDriver");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CodigoDispositivo", CodigoDispositivo);
            info.AddValue("ClaseDriver", ClaseDriver);
        }
    }
}
