using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("SuscripcionIdentificacionVehicular")]
    public class SuscripcionIdentificacionVehicular
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }

        [Column("ConfigIdentificacionVehicular_Id")]
        public virtual int ConfigIdentificacionVehicularId { get; set; }

        [ForeignKey("ConfigIdentificacionVehicularId")]
        public virtual ConfigIdentificacionVehicular ConfigIdentificacionVehicular { get; set; }

        [Required]
        [StringLength(200)]
        public virtual string CodigoEvento { get; set; }

        [Required]
        [StringLength(500)]
        public virtual string RutaAccesoSuscriptor { get; set; }
    }
}
