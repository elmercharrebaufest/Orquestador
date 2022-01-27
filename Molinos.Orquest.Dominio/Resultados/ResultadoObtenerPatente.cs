using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoObtenerPatente : ResultadoEjecutar
    {
        [DataMember]
        public byte[] Imagen { get; set; }
        [DataMember]
        public string Patente { get; set; }
        [DataMember]
        public float Confianza { get; set; }
    }
}
