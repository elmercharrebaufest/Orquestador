using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Web;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigTag")]
   public class ConfigTag : ConfigDispositivo
    {
        [Required]
        public virtual string Query { get; set; }

        public virtual int Timeout { get; set; }
    }
}
