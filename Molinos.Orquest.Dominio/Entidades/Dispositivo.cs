using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Dispositivo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Codigo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual string Codigo { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Descripcion")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual string Descripcion { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Activo")]
        public virtual bool Activo { get; set; }
        public virtual ConfigDispositivo Configuracion { get; set; }
        public virtual Orquestador TomadoPor { get; set; }
        public virtual Dispositivo Concentrador { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "EsConcentrador")]
        public virtual bool EsConcentrador { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(Textos), Name = "Concentrador")]
        public virtual int ConcentradorId { get; set; }

        public virtual bool EstadoCorrecto { get; set; }
    }
}
