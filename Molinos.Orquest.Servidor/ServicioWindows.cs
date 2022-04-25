using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Molinos.Orquest.Dependencias;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Servidor.Hosting;
using Molinos.Orquest.Servidor.Insights;
using Ninject;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servidor
{
    public partial class ServicioWindows : ServiceBase
    {
        private ServiceHost serviceHost;
        private ServiceHost sapServiceHost;
        private ServiceHost suscriptorServiceHost;

        public static IKernel KernelInstance { get; private set; }

        
        public ServicioWindows()
        {
            InitializeComponent();
        }

        internal void Start(string[] args)
        {
            this.OnStart(args);
        }

        protected override void OnStart(string[] args)
        {

            var fileInfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "/log4net.config");
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
            CreateKernel();
            var log = KernelInstance.Get<ILoggerFactory>().GetCurrentClassLogger();
            try
            {
                log.Debug("Iniciando Servicio...");

                if (serviceHost != null)
                {
                    serviceHost.Close();
                }
                log.Debug("Iniciando servicios web...");
                var urlOrquestador = String.Format(ConfigurationManager.AppSettings["UrlOrquestador"],
                                                   Environment.MachineName);
                var urlsOrquestador = urlOrquestador.Split(',').Select(url => new Uri(url)).ToArray();
                serviceHost = new NinjectServiceHost(KernelInstance, typeof (ServicioOrquestador), urlsOrquestador);

                TelemetryConfiguration.Active.TelemetryInitializers.Add(new RoleTelemetryInitializer());

                log.Debug("Inicializando orquestador...");
                ServicioOrquestador.Iniciar(Environment.MachineName, urlsOrquestador[0].AbsoluteUri);
                log.Debug("Servicio iniciado");

                serviceHost.Open();
                log.Debug("Servicio orquestador escuchando peticiones");

                var urlOrquestadorSAP = String.Format(ConfigurationManager.AppSettings["UrlOrquestadorSAP"],
                                                      Environment.MachineName);
                sapServiceHost = new NinjectServiceHost(KernelInstance, typeof (ServicioOrquestadorSAP),
                                                        new Uri(urlOrquestadorSAP));
                sapServiceHost.Open();
                log.Debug("Servicio orquestador SAP escuchando peticiones");
                Task.Run(() => ServicioOrquestador.VerificarDispositivos());

                suscriptorServiceHost = new ServiceHost(typeof (ServicioSuscriptor));
                suscriptorServiceHost.Open();
            }
            catch (ReflectionTypeLoadException rte)
            {
                log.Error(rte, "No se pudo iniciar el orquestador");
                foreach (var loaderException in rte.LoaderExceptions)
                {
                    log.Debug(loaderException, "Loader Exception");
                }
            }
            catch (Exception e)
            {
                log.Error(e, "No se pudo iniciar el orquestador");
                throw;
            }
        }

        protected override void OnStop()
        {
            var log = KernelInstance.Get<ILoggerFactory>().GetCurrentClassLogger();
            log.Debug("Deteniendo servicio...");
            suscriptorServiceHost.Close();
            sapServiceHost.Close();
            serviceHost.Close();
            ServicioOrquestador.Detener();
            log.Debug("Servicio detenido");
            DisposeKernel();
        }

        private IServicioOrquestador ServicioOrquestador 
        {
            get { return KernelInstance.Get<IServicioOrquestador>(); }
        }

        private void CreateKernel()
        {
            KernelInstance = new StandardKernel();
            KernelInstance.Load(new OrquestNinjectModule());
        }

        private void DisposeKernel()
        {
            if (KernelInstance != null)
            {
                KernelInstance.Dispose();
                KernelInstance = null;
            }
        }

    }
}
