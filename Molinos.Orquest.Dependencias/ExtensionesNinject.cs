using System.ServiceModel;
using System.ServiceModel.Description;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;

namespace Molinos.Orquest.Dependencias
{
    public static class ExtensionesNinject
    {
        internal static void BindChannelFactory<TChannel>(this NinjectModule module, string endpointConfigurationName, IEndpointBehavior behavior = null)
        {
            module.Bind<ChannelFactory<TChannel>>()
                .ToMethod(context => CreateChannelFactory<TChannel>(endpointConfigurationName, behavior))
                .InSingletonScope()
                .OnDeactivation(CloseCommunicationObject);

            module.Bind<TChannel>()
                .ToMethod(context => context.Kernel.Get<ChannelFactory<TChannel>>().CreateChannel())
                .InRequestScope()
                .OnDeactivation(channel => CloseCommunicationObject((ICommunicationObject)channel));
        }

        private static ChannelFactory<TChannel> CreateChannelFactory<TChannel>(string endpointConfigurationName, IEndpointBehavior behavior)
        {
            var factory = new ChannelFactory<TChannel>(endpointConfigurationName);
            if (behavior != null)
            {
                factory.Endpoint.Behaviors.Add(behavior);
            }
            return factory;
        }

        private static void CloseCommunicationObject(ICommunicationObject comObject)
        {
            try
            {
                if (comObject.State != CommunicationState.Faulted)
                {
                    comObject.Close();
                }
            }
            catch
            {
                try
                {
                    comObject.Abort();
                }
                catch
                {
                    // No hay nada para hacer
                }
            }
        }
    }
}
