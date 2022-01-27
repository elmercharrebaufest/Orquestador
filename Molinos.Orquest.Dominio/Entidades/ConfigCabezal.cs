using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Web;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigCabezal")]
    public class ConfigCabezal : ConfigDispositivo
    {
        [Required]
        public virtual string DireccionIp { get; set; }
        [Required]
        public virtual int Puerto { get; set; }
        [Required]
        public virtual int PosDesde { get; set; }
        [Required]
        public virtual int PosHasta { get; set; }
        [Required]
        public virtual int LongFrase { get; set; }
        public virtual string CarInicioFrase { get; set; }
        [Required]
        public virtual string ComandoPeso { get; set; }
        [Required]
        public virtual string ComandoCereo { get; set; }
        [Required]
        public virtual int CantLecPesoEstable { get; set; }
        [Required]
        public virtual int MaxCantLecPesoEstable { get; set; }
        [Required]
        public virtual int IntLecPesoEstable { get; set; }
        [Required]
        public virtual int IntLecCereo { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
        [Required]
        public virtual int DigitosDecimales { get; set; }
    }
}
