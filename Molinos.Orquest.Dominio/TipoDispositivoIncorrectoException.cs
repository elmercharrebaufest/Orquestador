using System;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio
{
    [Serializable]
    public class TipoDispositivoIncorrectoException : Exception
    {
        public string CodigoDispositivo { get; private set; }
        public string TipoDispositivo { get; private set; }

        public TipoDispositivoIncorrectoException(string mensaje, string codigoDispositivo, string tipoDispositivo) : base(mensaje)
        {
            CodigoDispositivo = codigoDispositivo;
            TipoDispositivo = tipoDispositivo;
        }

        protected TipoDispositivoIncorrectoException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            CodigoDispositivo = info.GetString("CodigoDispositivo");
            TipoDispositivo = info.GetString("TipoDispositivo");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CodigoDispositivo", CodigoDispositivo);
            info.AddValue("TipoDispositivo", TipoDispositivo);
        }
    }
}
