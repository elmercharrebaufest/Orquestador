using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigCartaLedModel")]
    public class ConfigCartelLedModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Cartel_Led_DireccionIp")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionIp { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Cartel_Led_Puerto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int Puerto { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_LecturaTimeout")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutLectura { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_LongFrase")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int LongFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_VelocidadScroll")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual int VelocidadScroll { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_Tipografia")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual int Tipografia { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_Brillo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual int ControlBrillo { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_Efecto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual int Efecto { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_NumeroTrama")]
        [RegularExpression("^[0-9]{2}$", ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_FormatoNumeroLed")]
        public virtual string NumeroTrama { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_NumeroVariable")]
        [RegularExpression("^[0-9]{2}$", ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_FormatoNumeroLed")]
        public virtual string NumeroVariable { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cartel_NumeroPrograma")]
        [RegularExpression("^[0-9]{2}$", ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_FormatoNumeroLed")]
        public virtual string NumeroPrograma { get; set; }
    }
}
