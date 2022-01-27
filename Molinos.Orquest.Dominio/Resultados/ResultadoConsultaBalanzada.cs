using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoConsultaBalanzada : ResultadoEjecutar
    {
        [DataMember]

        public Dictionary<string, string> ValoresBalanzada { get; set; }

        public ResultadoConsultaBalanzada()
        {
            ValoresBalanzada = new Dictionary<string, string>();
        }

        public ResultadoConsultaBalanzada Agregar(Dictionary<string,string> balanzada)
        {
            foreach (var b in balanzada) {
                ValoresBalanzada.Add(b.Key, b.Value);
            }            
            return this;
        }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append("Mensaje: ").Append(Mensaje).AppendLine(" - ")
                .Append("Valores: ");

            ImprimirValores(builder);
            builder.AppendLine();
            return builder.ToString();
        }

        private void ImprimirValores(StringBuilder builder)
        {
            builder.Append("[");
            foreach (var kv in ValoresBalanzada)
            {
                builder.AppendFormat("({0}: {1})", kv.Key, kv.Value);
            }
            builder.Append("]");
        }
    }
}
