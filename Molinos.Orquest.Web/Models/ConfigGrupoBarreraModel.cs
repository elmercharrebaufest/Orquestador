using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;

namespace Molinos.Orquest.Web.Models
{
    public class ConfigGrupoBarreraModel
    {
        public int Id { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Codigo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string Codigo { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Descripcion")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string Descripcion { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BarreraArriba")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int BarreraArribaId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BarreraAbajo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int BarreraAbajoId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "SensorArriba")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int SensorArribaId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "SensorAbajo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int SensorAbajoId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "SensorPrimerCruce")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int SensorPrimerCruceId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "SensorSegundoCruce")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int SensorSegundoCruceId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Cabezal_ClaseDriver")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string ClaseDriver { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Activo")]
        public bool Activo { get; set; }
    }
}
