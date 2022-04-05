using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigHumedimetroModel")]
    public class ConfigHumedimetroModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_DireccionIp")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionIp { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_Puerto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int Puerto { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_ComandoHumedad")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoHumedad { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_LongFrase")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int LongFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_LecturaTimeout")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutLectura { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_DelimitadorCampos")]
        [CharValidation]
        public string DelimitadorCampos { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_PosicionCampoHumedad")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosicionCampoHumedad { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_PosicionCampoPesoHectolitrico")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosicionCampoPesoHectolitrico{ get; set; }
    }
}
