using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigIdentificacionVehicular")]
    public class ConfigIdentificacionVehicular
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_Nombre")]
        [StringLength(100, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public virtual string Nombre { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_Codigo")]
        [StringLength(50, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public virtual string Codigo { get; set; }

        // Lector de tarjetas — disparador principal (opcional)
        [Column("ConfigLectorTarjetas_Id")]
        public virtual int? ConfigLectorTarjetasId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_LectorTarjetas")]
        [ForeignKey("ConfigLectorTarjetasId")]
        public virtual ConfigLectorTarjetas ConfigLectorTarjetas { get; set; }

        // Sensor vehicular — disparador (opcional, Etapa 2)
        [Column("ConfigSensorVehicular_Id")]
        public virtual int? ConfigSensorVehicularId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_SensorVehicular")]
        [ForeignKey("ConfigSensorVehicularId")]
        public virtual ConfigSensor ConfigSensorVehicular { get; set; }

        // Sensor de presencia — solo auditoría (opcional)
        [Column("ConfigSensorPresencia_Id")]
        public virtual int? ConfigSensorPresenciaId { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_SensorPresencia")]
        [ForeignKey("ConfigSensorPresenciaId")]
        public virtual ConfigSensor ConfigSensorPresencia { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_MaxReintentosFoto")]
        public virtual int MaxReintentosFoto { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "ConfigIdentificacionVehicular_DelayEntreReintentosMs")]
        public virtual int DelayEntreReintentosMs { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Activo")]
        public virtual bool Activo { get; set; }

        [Column("TomadoPor_Id")]
        public virtual int? TomadoPorId { get; set; }

        [ForeignKey("TomadoPorId")]
        public virtual Orquestador TomadoPor { get; set; }

        public virtual ICollection<ConfigIdentificacionVehicularCamara> Camaras { get; set; }
    }
}
