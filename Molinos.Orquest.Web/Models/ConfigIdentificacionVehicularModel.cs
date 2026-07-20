using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Web.Models
{
    public class ConfigIdentificacionVehicularModel
    {
        public int Id { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_Nombre")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(100, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string Nombre { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_Codigo")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string Codigo { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_LectorTarjetas")]
        public int? ConfigLectorTarjetasId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_SensorPresencia")]
        public int? ConfigSensorVehicularId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_SensorPresencia")]
        public int? ConfigSensorPresenciaId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_MaxReintentosFoto")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Range(0, 10, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_RangoNumerico")]
        public int MaxReintentosFoto { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_DelayEntreReintentosMs")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public int DelayEntreReintentosMs { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Activo")]
        public bool Activo { get; set; }

        public IList<CamaraItemModel> Camaras { get; set; } = new List<CamaraItemModel>();

        public class CamaraItemModel
        {
            public int Id { get; set; }
            public string Nombre { get; set; }
            public string Ip { get; set; }
        }
    }
}
