using Molinos.Orquest.DriversImpl.ServicioALPR;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Ninject.Modules;
using System.Data.Entity;

namespace Molinos.Orquest.Dependencias
{
    public class OrquestNinjectModule : NinjectModule
    {
        public override void Load()
        {
            Bind<DbContext>().To<OrquestadorDbContext>().InTransientScope();
            Bind<IRepositorio>().To<RepositorioEF>().InTransientScope();
            this.BindChannelFactory<IServicioALPR>("ServicioALPR");

            Bind<IServicioOrquestadorSAP, ServicioOrquestadorSAP>().To<ServicioOrquestadorSAP>().InSingletonScope();
            Bind<IServicioOrquestador, ServicioOrquestador>().To<ServicioOrquestador>().InSingletonScope();
            Bind<IDriverFactory>().To<DriverFactory>().InSingletonScope();
            Bind<IProcesadorFactory>().To<ProcesadorFactory>().InSingletonScope();
            Bind<IServicioRemotoFactory>().To<ServicioRemotoFactory>().InSingletonScope();
            Bind<IRepositorioFactory>().To<RepositorioFactory>().InSingletonScope();
            Bind<INamedLocker>().To<NamedLocker>().InSingletonScope();
            Bind<IAdministradorSuscripciones>().To<AdministradorSuscripciones>().InSingletonScope();
            Bind<IProgramadorTareas>().To<ProgramadorTareas>().InSingletonScope();
        }
    }
}
