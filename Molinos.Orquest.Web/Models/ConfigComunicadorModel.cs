using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigComunicador")]
    public class ConfigComunicadorModel : ConfigDispositivo
    {
        public int NumeroSalida { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Intercomunicador_TiempoMaximoEjecucion")]
        public int? TiempoMaximoEjecucion { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Intercomunicador_PuertoAudio")]
        public int? PuertoDeAudio { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Intercomunicador_SensorDeEstado")]
        public int? Sensor_Id { get; set; }
        public Dispositivo Sensor { get; set; }
    }
}