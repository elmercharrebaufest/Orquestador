using Molinos.Orquest.DriversImpl.ServicioALPR;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Molinos.Orquest.Dependencias
{
    public class OrquestNinjectModule : NinjectModule
    {
        public IDictionary<string, string> AppConfig { get; set; } = new Dictionary<string, string>();

        public override void Load()
        {
            int maxNotificacions = 0;
            if (AppConfig.TryGetValue("maxNotificacions", out var maxNotificacionsString))
            {
                if (!int.TryParse(maxNotificacionsString, out maxNotificacions))
                {
                    throw new ArgumentException($"El valor de configuración `maxNotificacions` es invalido `{maxNotificacionsString}`. Debe ser un valor numerico en minutos.");
                }
            }

            if (maxNotificacions > 0)
            {
                // Creo una instancia de ControlDeNotificaciones para decore a AdministradorSuscripciones.
                Bind<IAdministradorSuscripciones>().To<AdministradorSuscripciones>().WhenInjectedInto<ControlDeNotificaciones>().InSingletonScope();
                Bind<IAdministradorSuscripciones>().To<ControlDeNotificaciones>().InSingletonScope().WithConstructorArgument("maxNotificacions", maxNotificacions);
            }
            else
            {
                // Se ignora el uso de ControlDeNotificaciones.
                Bind<IAdministradorSuscripciones>().To<AdministradorSuscripciones>().InSingletonScope();
            }


            Bind<DbContext>().To<OrquestadorDbContext>().InTransientScope();
            Bind<IRepositorio>().To<RepositorioEF>().InTransientScope();

            Bind<IServicioOrquestadorSAP, ServicioOrquestadorSAP>().To<ServicioOrquestadorSAP>().InSingletonScope();
            Bind<IServicioOrquestador, ServicioOrquestador>().To<ServicioOrquestador>().InSingletonScope();
            Bind<IDriverFactory>().To<DriverFactory>().InSingletonScope();
            Bind<IProcesadorFactory>().To<ProcesadorFactory>().InSingletonScope();
            Bind<IServicioRemotoFactory>().To<ServicioRemotoFactory>().InSingletonScope();
            Bind<IRepositorioFactory>().To<RepositorioFactory>().InSingletonScope();
            Bind<INamedLocker>().To<NamedLocker>().InSingletonScope();

            
            
            Bind<IProgramadorTareas>().To<ProgramadorTareas>().InSingletonScope();
        }
    }
}
