using System;
using System.ComponentModel.DataAnnotations;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Firma
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual Byte[] Logo { get; set; }
        public virtual Byte[] Favicon { get; set; }
    }
}
