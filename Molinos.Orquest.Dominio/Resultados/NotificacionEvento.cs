using System.Collections.Generic;
using System.Text;

namespace Molinos.Orquest.Dominio.Resultados
{
    public class NotificacionEvento
    {
        public string CodigoDispositivo { get; set; }
        public string CodigoEvento { get; set; }
        public Dictionary<string, decimal> Valores { get; set; }
        public Dictionary<string, string> Datos { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder()
                .Append("Dispositivo: ").Append(CodigoDispositivo).Append('|')
                .Append("Evento: ").Append(CodigoEvento).Append('|');
            builder.Append("Datos: [");
            if (Datos != null)
            {
                foreach (var valor in Datos)
                {
                    builder.Append(valor.Key).Append('=').Append(valor.Value).Append(';');
                }
            }
            builder.Append("]");
            builder.Append("Valores: [");
            if (Valores != null)
            {
                foreach (var valor in Valores)
                {
                    builder.Append(valor.Key).Append('=').Append(valor.Value).Append(';');
                }
            }
            builder.Append("]");
            return builder.ToString();
        }
    }
}