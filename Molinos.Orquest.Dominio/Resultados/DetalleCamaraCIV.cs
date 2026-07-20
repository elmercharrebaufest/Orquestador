using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class DetalleCamaraCIV
    {
        [DataMember]
        public string CodigoCamara { get; set; }

        [DataMember]
        public string ProveedorALPR { get; set; }

        [DataMember]
        public int Intentos { get; set; }

        [DataMember]
        public string Patente { get; set; }

        [DataMember]
        public string RutaImagen { get; set; }

        [DataMember]
        public float? Certeza { get; set; }

        [DataMember]
        public string Error { get; set; }
    }
}
