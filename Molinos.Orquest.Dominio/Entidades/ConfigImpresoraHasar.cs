using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigImpresoraHasar")]
    public class ConfigImpresoraHasar : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
    }
}
