using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarConsultaBalanzada : ComandoEjecutar
    {
        [DataMember]
        public int? IdBalanzada { get; set; }
        public override string ToString()
        {
            return "Ejecutar Consultar Balanzada: " + IdBalanzada == null? "última" : IdBalanzada + ", Balanza: " + CodigoDispositivo;
        }
    }
}
