using System;
using System.ServiceModel;
using Ninject;

namespace Molinos.Orquest.Servidor.Hosting
{
    public class NinjectServiceHost : ServiceHost
    {
        private readonly IKernel kernel;


        public NinjectServiceHost()
        {
        }

        public NinjectServiceHost(IKernel kernel, Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses)
        {
            this.kernel = kernel;
        }

        protected override void OnOpening()
        {
            Description.Behaviors.Add(new NinjectInstanceProviderServiceBehavior(kernel));
            base.OnOpening();
        }
    }
}
