using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using Ninject;

namespace Molinos.Orquest.Servidor.Hosting
{
    public class NinjectInstanceProviderServiceBehavior : IServiceBehavior
    {
        private readonly IKernel kernel;

        public NinjectInstanceProviderServiceBehavior(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
                                         BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (var channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                var dispatcher = channelDispatcher as ChannelDispatcher;

                if (dispatcher != null)
                {
                    foreach (var endpoint in dispatcher.Endpoints)
                    {
                        endpoint.DispatchRuntime.InstanceProvider = new NinjectInstanceProvider(serviceDescription.ServiceType, kernel);  
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

    }
}
