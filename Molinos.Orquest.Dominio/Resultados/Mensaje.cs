using System.Globalization;
using System.Runtime.Serialization;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class Mensaje
    {
        [DataMember]
        public int Codigo { get; set; }
        [DataMember]
        public string Descripcion { get; set; }

        public Mensaje(int codigo, string descripcion, params object[] argumentos)
        {
            Codigo = codigo;
            Descripcion = string.Format(descripcion, argumentos);
        }

        public override string ToString()
        {
            return Codigo.ToString(CultureInfo.InvariantCulture) + " - " + Descripcion;
        }

        public static Mensaje ResultadoOK()
        {
            return new Mensaje(Codigos.OK, Textos.ResultadoOK);
        }
    }
}
