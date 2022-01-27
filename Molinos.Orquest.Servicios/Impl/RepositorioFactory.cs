using Molinos.Orquest.Repositorio;
using Ninject;

namespace Molinos.Orquest.Servicios.Impl
{
    public class RepositorioFactory : IRepositorioFactory
    {
        private readonly IKernel kernel;

        public RepositorioFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IRepositorio Repositorio()
        {
            return kernel.Get<IRepositorio>();
        }
    }
}
