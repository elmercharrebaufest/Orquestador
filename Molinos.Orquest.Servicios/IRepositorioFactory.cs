using Molinos.Orquest.Repositorio;

namespace Molinos.Orquest.Servicios
{
    public interface IRepositorioFactory
    {
        IRepositorio Repositorio();
    }
}
