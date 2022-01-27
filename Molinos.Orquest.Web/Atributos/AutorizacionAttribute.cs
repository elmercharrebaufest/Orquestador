using System;
using System.Security.Authentication;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Seguridad;

namespace Molinos.Orquest.Web.Atributos
{
    public sealed class AutorizacionAttribute : AuthorizeAttribute
    {
        public AutorizacionAttribute(params PermisosOrquestador[] permisos)
        {
            base.Roles = string.Join(", ", permisos);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations"), Obsolete("Usar el constructor con la lista de PermisosOrquestador.", true)]
        public new string Roles
        {
            get { throw new AuthenticationException("No usar esta propiedad. Usar el constructor con la lista de PermisosOrquestador."); }
            set { throw new AuthenticationException("No usar esta propiedad. Usar el constructor con la lista de PermisosOrquestador."); }
        }
    }
}