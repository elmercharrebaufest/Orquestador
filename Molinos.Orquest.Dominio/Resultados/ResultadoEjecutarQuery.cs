using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
   public class ResultadoEjecutarQuery : ResultadoEjecutar
    {
        [DataMember]
        public List<string> queryResult { get; set; }
    }
}
