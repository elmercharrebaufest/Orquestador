using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigHumedimetro")]
    public class ConfigHumedimetro : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        public virtual string ComandoHumedad { get; set; }
        [Required]
        public virtual int LongFrase { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
        public virtual string DelimitadorCampos { get; set; }
        [Required]
        public virtual int PosicionCampoHumedad { get; set; }
    }
}
