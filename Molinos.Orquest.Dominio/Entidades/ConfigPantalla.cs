using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigPantalla")]
    public class ConfigPantalla : ConfigDispositivo
    {
        [Required]
        public virtual string UrlSCATO { get; set; }
        
        [Required]
        public virtual string UrlFTP { get; set; }

        [Required]
        public virtual int TiempoDeRefresco { get; set; }
        [Required]
        public virtual string NombreUsuario { get; set; }
        [Required]
        public virtual string Contrasenia { get; set; }
    }
}
