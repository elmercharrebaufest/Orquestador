using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigBarrera")]
    public class ConfigBarrera : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_NumeroSalida")]
        public virtual int NumeroSalida { get; set; }
        public virtual bool EstadoAbierta { get; set; }
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_TiempoActivacion")]
        public virtual int TiempoActivacion { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_EstadoAbierta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Range(0, 1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_SoloCeroUno")]
        public virtual int SenalDeActivacion
        {
            get { return EstadoAbierta ? 1 : 0; }
            set { EstadoAbierta = value != 0; }
        }

        public virtual ConfigSensor Sensor { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "Sensor")]
        public virtual int? SensorId { get; set; }

        [Range(0, 60, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_RangoNumerico")]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_TiempoEsperaReintento")]
        public virtual int? TiempoEsperaReintento { get; set; }

        [Range(0, 60, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_RangoNumerico")]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_TiempoMaximoEsperaReintento")]
        public virtual int? TiempoMaximoEsperaReintento { get; set; }
    }
}
