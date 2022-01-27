using System;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Test.Mocks
{
    public sealed class NullLoggerFactory : ILoggerFactory
    {
        public INinjectSettings Settings { get; set; }

        public ILogger GetLogger(Type type)
        {
            return new NullLogger();
        }

        public ILogger GetLogger(IContext context)
        {
            return new NullLogger();
        }

        public ILogger GetCurrentClassLogger()
        {
            return new NullLogger();
        }

        public void Dispose()
        {
        }
    }
}
