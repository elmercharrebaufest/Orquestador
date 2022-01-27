using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigSensor")]
    public class ConfigSensor : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "NumeroEntrada")]
        public virtual int NumeroEntrada { get; set; }
        public virtual bool EstadoActivado { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_EstadoAbierta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Range(0, 1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_SoloCeroUno")]
        public virtual int SenalDeActivacion
        {
            get { return EstadoActivado ? 1 : 0; } 
            set { EstadoActivado = value != 0; }
        }
    }
}
