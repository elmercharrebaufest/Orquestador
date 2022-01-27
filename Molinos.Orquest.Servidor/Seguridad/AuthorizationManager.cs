using System;
using System.Linq;
using System.ServiceModel;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Servicios;
using Ninject;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servidor.Seguridad
{
    public class AuthorizationManager : ServiceAuthorizationManager
    {
        private readonly IRepositorioFactory repositorioFactory;
        private readonly ILogger log;

        public AuthorizationManager()
        {
            repositorioFactory = ServicioWindows.KernelInstance.Get<IRepositorioFactory>();
            log = ServicioWindows.KernelInstance.Get<ILoggerFactory>().GetCurrentClassLogger();
        }
        
        protected override bool CheckAccessCore(OperationContext operationContext)
        {
            string usuarioDominio = null;
            var autorizado = false;
            try
            {
                usuarioDominio = operationContext.ServiceSecurityContext.WindowsIdentity.Name;
                if (usuarioDominio != null && usuarioDominio.Contains("\\"))
                {
                    var nombreUsuario = usuarioDominio.Split('\\')[1];
                    using (var repositorio = repositorioFactory.Repositorio())
                    {
                        autorizado = repositorio.Existe<Usuario>(u => u.NombreUsuario == nombreUsuario && u.RolesAsociados.Any(x => x.PermisosAsociados.Any(y => y.PermisoOrquestador == PermisosOrquestador.ServicioOrquestador)));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "No se pudo authorizar al usuario: {0}", usuarioDominio);
            }
            return autorizado;
        }

    }
}
