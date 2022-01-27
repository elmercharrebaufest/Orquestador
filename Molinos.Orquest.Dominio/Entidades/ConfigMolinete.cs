using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigMolinete")]
    public class ConfigMolinete : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionUrl { get; set; }
        [Required]
        public virtual int TimeoutHabilitacion { get; set; }
        [Required]
        public virtual int IntervaloPooling { get; set; }
    }
}
