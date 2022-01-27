using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoBorrarBalanzadasPorRango : ResultadoEjecutar
    {
        [DataMember]
        public int IdBalanzadaInicio { get; set; }
        public int IdBalanzadaFin { get; set; }

        public Dictionary<string, bool> BalanzadasBorradas { get; set; }
    }
}
