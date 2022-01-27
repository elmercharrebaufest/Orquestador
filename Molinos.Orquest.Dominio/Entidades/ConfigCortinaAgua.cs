using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigCortinaAgua")]
    public class ConfigCortinaAgua : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_IntervaloPooling")]
        public virtual int IntervaloPooling { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_DireccionVientoDesde")]
        public virtual int DireccionDelVientoDesde { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_DireccionVientoHasta")]
        public virtual int DireccionDelVientoHasta { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_NumeroSalida")]
        public virtual int NumeroSalida { get; set; }
       
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_TiempoActivacion")]
        public virtual int? TiempoActivacion { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_TiempoEsperaActivacion")]
        public virtual int? TiempoDeEsperaActivacion { get; set; }

        public virtual bool EstadoAbierta { get; set; }

        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_EstadoAbierta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Range(0, 1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_SoloCeroUno")]
        public virtual int SenalDeActivacion
        {
            get { return EstadoAbierta ? 1 : 0; }
            set { EstadoAbierta = value != 0; }
        }

        public virtual ConfigMeteorologica Estacion { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "Estacion")]
        public virtual int? EstacionId { get; set; }
    }
}
