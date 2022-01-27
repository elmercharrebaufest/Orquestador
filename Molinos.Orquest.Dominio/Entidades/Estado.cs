using System;
using System.ComponentModel.DataAnnotations;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Estado
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual DateTime Fecha { get; set; }
    }
}
