using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoEjecutar : ResultadoComando
    {
        [DataMember]
        public Dictionary<string, decimal> Valores { get; set; }

        public ResultadoEjecutar()
        {
            Valores = new Dictionary<string, decimal>();
        }
        
        public ResultadoEjecutar Agregar(string clave, decimal valor)
        {
            Valores.Add(clave, valor);
            return this;
        }

        public ResultadoEjecutar Agregar(Dictionary<string, decimal> valores)
        {
            foreach(var valor in valores)
            {
                if (Valores.ContainsKey(valor.Key))
                {
                    Valores[valor.Key] = valor.Value;
                }
                else
                {
                    Valores.Add(valor.Key, valor.Value);
                }
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
            foreach (var kv in Valores)
            {
                builder.AppendFormat("({0}: {1})", kv.Key, kv.Value);
            }
            builder.Append("]");
        }
    }
}
