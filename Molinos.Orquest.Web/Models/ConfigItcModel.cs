using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigItcModel")]
    public class ConfigItcModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_DireccionIp")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(500, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionIp { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_Puerto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int Puerto { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_LongFrase")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int LongFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_LecturaTimeout")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutLectura { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_IntervaloPolling")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int IntervaloPolling { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_ComandoEstado")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoEstado { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_ComandoTarjeta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoTarjeta { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_ComandoActivarSalida")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoActivarSalida { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_CarInicioFrase")]
        [CharValidation]
        public string CarInicioFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_CarFinFrase")]
        [CharValidation]
        public string CarFinFrase { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_DelimitadorCampos")]
        [CharValidation]
        public string DelimitadorCampos { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_RespuestaExito")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string RespuestaExito { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_RespuestaError")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string RespuestaError { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Itc_EstadoDispositivo")]
        public bool EstaConectado { get; set; }
        public string MensajeError { get; set; }
    }
}
