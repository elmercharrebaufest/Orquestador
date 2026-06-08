using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigIdentificacionVehicularCamara")]
    public class ConfigIdentificacionVehicularCamara
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        [Column("ConfigIdentificacionVehicular_Id")]
        public virtual int ConfigIdentificacionVehicularId { get; set; }

        [ForeignKey("ConfigIdentificacionVehicularId")]
        public virtual ConfigIdentificacionVehicular ConfigIdentificacionVehicular { get; set; }

        [Column("ConfigCamara_Id")]
        public virtual int ConfigCamaraId { get; set; }

        [ForeignKey("ConfigCamaraId")]
        public virtual ConfigCamara ConfigCamara { get; set; }
    }
}
