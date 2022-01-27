using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigHistorian")]
    public  class ConfigHistorian : ConfigDispositivo
    {
       

        [Required]
        public virtual string ConectionString { get; set; }
        [Required]
        public virtual int IdDispositivo { get; set; }
    }
}
