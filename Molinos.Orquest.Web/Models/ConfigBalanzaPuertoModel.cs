using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigBalanzaPuertoModel")]
    public class ConfigBalanzaPuertoModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_DireccionIp")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string DireccionIp { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_Puerto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int Puerto { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_PosDesde")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosDesde { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_PosHasta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int PosHasta { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_LongFrase")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int LongFrase { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_LecturaTimeout")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int TimeoutLectura { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_ComandoConsulta")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string ComandoConsulta { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_ComandoBorrado")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(1, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string ComandoBorrado { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_CantidadCaracteresTotal")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int CantidadCaracteresTotal { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_CaracterIzquierdaACompletar")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string CaracterIzquierdaACompletar { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_IntervaloPolling")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int IntervaloPolling { get; set; }


    }
}
