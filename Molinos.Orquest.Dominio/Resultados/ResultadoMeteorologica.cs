using Molinos.Orquest.Dominio.Dtos;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoMeteorologica: ResultadoEjecutar
    {
        [DataMember]
        public List<MeteorologicaDto> Imagenes { get; set; }
    }
}
