using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigPantallaModel")]
    public class ConfigPantallaModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Pantalla_UrlSCATO")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(250, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string UrlSCATO { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Pantalla_UrlFTP")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(250, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string UrlFTP { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Pantalla_TiempoDeRefresco")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TiempoDeRefresco { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Nombre_Usuario")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string NombreUsuario { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Contrasenia")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string Contrasenia { get; set; }


    }
}
