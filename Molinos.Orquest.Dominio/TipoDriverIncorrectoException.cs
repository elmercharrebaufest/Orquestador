using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio
{
    [Serializable]
    public class TipoDriverIncorrectoException : Exception
    {
        public string CodigoDispositivo { get; set; }
        public string ClaseDriver { get; set; }

        public TipoDriverIncorrectoException(string codigoDispositivo, string claseDriver)
        {
            CodigoDispositivo = codigoDispositivo;
            ClaseDriver = claseDriver;
        }

        public TipoDriverIncorrectoException(string message, string codigoDispositivo, string claseDriver) : base(message)
        {
            CodigoDispositivo = codigoDispositivo;
            ClaseDriver = claseDriver;
        }

        public TipoDriverIncorrectoException(string message, Exception innerException, string codigoDispositivo, string claseDriver) : base(message, innerException)
        {
            CodigoDispositivo = codigoDispositivo;
            ClaseDriver = claseDriver;
        }

        protected TipoDriverIncorrectoException(SerializationInfo info, StreamingContext context): base(info, context)
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
