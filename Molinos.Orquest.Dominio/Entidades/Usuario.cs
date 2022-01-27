using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Usuario
    {
        [Key]
        public virtual int Id { get; set; }
        [Required]
        public virtual string NombreUsuario { get; set; }
        [Required]
        public virtual string Apellido { get; set; }
        [Required]
        public virtual string Nombre { get; set; }

        [InverseProperty("UsuariosAsociados")]
        public virtual ICollection<Rol> RolesAsociados { get; set; }
    }
}
