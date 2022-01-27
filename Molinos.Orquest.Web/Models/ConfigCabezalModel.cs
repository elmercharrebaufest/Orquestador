using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigCabezalModel")]
    public class ConfigCabezalModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_DireccionIp")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionIp { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_Puerto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int Puerto { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_PosDesde")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosDesde { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_PosHasta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosHasta { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_LongFrase")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int LongFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_CarInicioFrase")]
        public string CarInicioFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_ComandoPeso")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoPeso { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_ComandoCereo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoCereo { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_CantLecPesoEstable")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int CantLecPesoEstable { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_MaxCantLecPesoEstable")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int MaxCantLecPesoEstable { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_IntLecPesoEstable")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int IntLecPesoEstable { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_IntLecCereo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int IntLecCereo { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_LecturaTimeout")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutLectura { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_DigitosDecimales")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int DigitosDecimales { get; set; }
    }
}
