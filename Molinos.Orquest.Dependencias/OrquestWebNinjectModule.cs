using System.Data.Entity;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Ninject.Modules;

namespace Molinos.Orquest.Dependencias
{
    public class OrquestWebNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<OrquestadorDbContext>().InTransientScope();
            Bind<IRepositorio>().To<RepositorioEF>().InTransientScope();
            Bind<IRepositorioFactory>().To<RepositorioFactory>().InSingletonScope();
            Bind<IDriverFactory>().To<DriverFactory>().InSingletonScope();
            this.BindChannelFactory<IServicioOrquestador>("Orquestador");
        }
    }
}
