using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigItc")]
    public class ConfigItc : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        [Required]
        public virtual int LongFrase { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
        [Required]
        public virtual int IntervaloPolling { get; set; }
        [Required]
        public virtual string ComandoEstado { get; set; }
        [Required]
        public virtual string ComandoTarjeta { get; set; }
        [Required]
        public virtual string ComandoActivarSalida { get; set; }
        public virtual string CarInicioFrase { get; set; }
        public virtual string CarFinFrase { get; set; }
        public virtual string DelimitadorCampos { get; set; }
        [Required]
        public virtual string RespuestaExito { get; set; }
        [Required]
        public virtual string RespuestaError { get; set; }
    }
}
