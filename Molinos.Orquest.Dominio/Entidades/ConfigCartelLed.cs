using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigCartelLed")]
    public class ConfigCartelLed : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }       
        [Required]
        public virtual int LongFrase { get; set; }       
        [Required]
        public virtual int VelocidadScroll { get; set; }
        [Required]
        public virtual int Tipografia { get; set; }
        [Required]
        public virtual int ControlBrillo { get; set; }
        [Required]
        public virtual int Efecto { get; set; }
        public virtual string NumeroTrama { get; set; }
        public virtual string NumeroVariable { get; set; }
        public virtual string NumeroPrograma { get; set; }
    }
}
