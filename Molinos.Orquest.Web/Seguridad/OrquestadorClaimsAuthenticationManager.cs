using System;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security;
using System.Security.Claims;
using System.Text;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Repositorio;
using Ninject.Extensions.Logging;
using WebGrease.Css.Extensions;

namespace Molinos.Orquest.Web.Seguridad
{
    public class OrquestadorClaimsAuthenticationManager : ClaimsAuthenticationManager
    {
        private readonly IRepositorio repositorio;
        private readonly ILogger log;

        private OrquestadorClaimsAuthenticationManager(IRepositorio repositorio)
        {
            this.repositorio = repositorio;
            var loggerFactory = DependencyResolver.Current.GetService<ILoggerFactory>();
            log = loggerFactory.GetCurrentClassLogger();
        }

        public OrquestadorClaimsAuthenticationManager()
            : this(DependencyResolver.Current.GetService<IRepositorio>())
        {
        }

        public override ClaimsPrincipal Authenticate(string resourceName, ClaimsPrincipal incomingPrincipal)
        {
            if (incomingPrincipal == null || !incomingPrincipal.Identity.IsAuthenticated)
            {
                return incomingPrincipal;
            }
            log.Debug("Recibida autenticación de usuario");
            var identity = ((ClaimsIdentity) incomingPrincipal.Identity);
            var claim = identity.Claims.SingleOrDefault(x => x.Type == ClaimTypes.Name);
            if (claim == null)
            {
                var sb = new StringBuilder();
                identity.Claims.ForEach(x => sb.Append(x.Type + ": " + x.Value + "\n"));
                log.Error("No se encontró el nombre de usuario en los claims recibidos. No se puede continuar con la autenticación. Claims recibidos: {0}",sb);
                throw new SecurityException(string.Format("No se encontró el nombre de usuario en los claims recibidos. No se puede continuar con la autenticación. Claims recibidos: {0}",sb));
            }
            log.Debug("Claim Value: {0}", claim.Value);
            var nombreUsuario = claim.Value.Split('\\')[1];
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, nombreUsuario));

            log.Debug("Agregando claims de permisos del Orquestador para el usuario {0}", nombreUsuario);

            var usuario = repositorio.ObtenerNoTracking<Usuario>(u => u.NombreUsuario == nombreUsuario);
            if (usuario != null)
            {
                foreach (var permiso in usuario.RolesAsociados.SelectMany(rol => rol.PermisosAsociados).Distinct())
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, permiso.PermisoOrquestador.ToString()));
                    log.Debug("Agregando permiso {0} para el usuario {1}", permiso.PermisoOrquestador.DisplayEnum(), nombreUsuario);
                }
            }

            var ci = new ClaimsIdentity(((ClaimsIdentity)incomingPrincipal.Identity).Claims, "Negotiate");
            var transformedPrincipal = new ClaimsPrincipal(ci);
            CreateSession(transformedPrincipal);
            return transformedPrincipal;
        }

        private void CreateSession(ClaimsPrincipal transformedPrincipal)
        {
            var sessionSecurityToken = new SessionSecurityToken(transformedPrincipal, TimeSpan.FromHours(8));
            FederatedAuthentication.SessionAuthenticationModule.WriteSessionTokenToCookie(sessionSecurityToken);
        }
    }
}