using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigNirs")]
    public class ConfigNirs : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
        [Required]
        public virtual string CaracteresABorrar { get; set; }
    }
}
