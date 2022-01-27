using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Web;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigBalanzaPuerto")]
    public class ConfigBalanzaPuerto : ConfigDispositivo
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
        [Required]
        public virtual int IntervaloPolling { get; set; }
        [Required]
        public virtual int TimeoutLectura { get; set; }
        [Required]
        public virtual string ComandoConsulta { get; set; }
        [Required]
        public virtual string ComandoBorrado { get; set; }
        [Required]
        public virtual int CantidadCaracteresTotal { get; set; }
        [Required]
        public virtual string CaracterIzquierdaACompletar { get; set; }

    }
}
