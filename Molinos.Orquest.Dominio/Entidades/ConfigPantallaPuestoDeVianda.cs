using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigPantallaPuestoDeVianda")]
    public class ConfigPantallaPuestoDeVianda : ConfigDispositivo
    {
        public virtual ConfigPuestoDeVianda ConfigPuestoDeVianda { get; set; }
    }
}
