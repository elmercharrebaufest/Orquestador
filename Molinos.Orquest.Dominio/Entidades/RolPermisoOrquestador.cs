using System.ComponentModel.DataAnnotations;
using Molinos.Orquest.Dominio.Seguridad;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class RolPermisoOrquestador
    {
        [Key]
        public virtual int Id { get; set; }
        public virtual Rol Rol { get; set; }
        public virtual PermisosOrquestador PermisoOrquestador { get; set; }
    }
}
