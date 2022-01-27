using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigMolineteModel")]
    public class ConfigMolineteModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Molinete_DireccionUrl")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(300, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionUrl { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Molinete_TimeoutHabilitacion")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutHabilitacion { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Molinete_IntervaloPooling")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int IntervaloPooling { get; set; }
    }
}