using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Molinos.Orquest.Web.ServicioHub
{
    public class HubClientFactory
    {
        private readonly IKernel kernel;

        public HubClientFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public HubClient GetClient()
        {
            return kernel.Get<HubClient>();
        }
    }
}