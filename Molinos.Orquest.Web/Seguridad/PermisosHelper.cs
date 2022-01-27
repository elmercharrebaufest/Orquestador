using System.Linq;
using System.Security.Claims;
using Molinos.Orquest.Dominio.Seguridad;

namespace Molinos.Orquest.Web.Seguridad
{
    public class PermisosHelper
    {
        public static bool Is(params PermisosOrquestador[] permisos)
        {
            var permisosUsuario = ClaimsPrincipal.Current.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
            return permisos.Any(p => permisosUsuario.Contains(p.ToString()));
        }

        public static bool Any()
        {
            var permisosUsuario = ClaimsPrincipal.Current.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);
            return permisosUsuario.Any(x => x != PermisosOrquestador.ServicioOrquestador.ToString());
        }
    }
}