using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    public class ResultadoEstadoSensor : ResultadoEjecutar
    {
        [DataMember]
        public string CodigoDispositivoConcentrador { get; set; }
        [DataMember]
        public string CodigoDispositivoSensor { get; set; }        
        [DataMember]
        public bool EstadoActivo { get; set; }
    }
}
