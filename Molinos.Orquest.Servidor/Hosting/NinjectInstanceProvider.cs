using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using Ninject;

namespace Molinos.Orquest.Servidor.Hosting
{
    public class NinjectInstanceProvider : IInstanceProvider
    {
        private readonly Type serviceType;
        private readonly IKernel kernel;

        public NinjectInstanceProvider(Type serviceType, IKernel kernel)
        {
            this.serviceType = serviceType;
            this.kernel = kernel;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return kernel.Get(serviceType);
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            var disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
