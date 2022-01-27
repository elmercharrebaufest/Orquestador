using Molinos.Orquest.Dependencias;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Conversiones.Impl;
using Molinos.Orquest.Web.ServicioHub;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Molinos.Orquest.Web.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(Molinos.Orquest.Web.App_Start.NinjectWebCommon), "Stop")]

namespace Molinos.Orquest.Web.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Load(new OrquestWebNinjectModule());
            kernel.Bind<IConversor>().To<ConversorAutoMapper>().InSingletonScope();
            kernel.Bind<IServicioSuscriptor>().To<ServicioSuscriptor>();

            kernel.Bind<HubClient>().ToSelf().InSingletonScope();
            kernel.Bind<HubClientFactory>().ToSelf().InSingletonScope();
        }        
    }
}
