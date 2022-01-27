

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigPuestoDeVianda")]
    public class ConfigPuestoDeVianda : ConfigDispositivo
    {
        [Required]
        public string Sector { get; set; }
        [InverseProperty("ConfigPuestoDeVianda")]
        public virtual ICollection<ConfigPantallaPuestoDeVianda> ConfigPantallaPuestoDeViandas { get; set; }
    }
}