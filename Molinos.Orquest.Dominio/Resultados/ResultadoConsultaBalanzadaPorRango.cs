using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoConsultaBalanzadaPorRango : ResultadoEjecutar
    {
        [DataMember]

        public List<ResultadoConsultaBalanzada> Balanzadas;

        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append("Mensaje: ").Append(Mensaje).AppendLine(" - ")
                .Append("Mensaje por balanzada: ");

            ImprimirValores(builder);
            builder.AppendLine();
            return builder.ToString();
        }

        private void ImprimirValores(StringBuilder builder)
        {
            builder.Append("[");
            foreach (var b in Balanzadas)
            {
                builder.AppendFormat("({0})", b.Mensaje);
            }
            builder.Append("]");
        }

    }
}
