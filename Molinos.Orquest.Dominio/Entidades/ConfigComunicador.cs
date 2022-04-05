using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigComunicador")]
    public class ConfigComunicador : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Barrera_NumeroSalida")]
        public virtual int NumeroSalida { get; set; }

        public virtual int? TiempoMaximoEjecucion { get; set; }
        public virtual int? PuertoDeAudio { get; set; }

        [Column("Sensor_Id")]
        public virtual int? SensorId { get; set; }
        public virtual Dispositivo Sensor { get; set; }
    }
}