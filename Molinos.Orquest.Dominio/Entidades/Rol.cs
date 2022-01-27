using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Rol
    {
        [Key]
        public virtual int Id { get; set; }
        [Required]
        public virtual string Descripcion { get; set; }

        public virtual ICollection<RolPermisoOrquestador> PermisosAsociados { get; set; }
        public virtual ICollection<Usuario> UsuariosAsociados { get; set; }
    }
}
