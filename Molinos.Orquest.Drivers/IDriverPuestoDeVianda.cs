using Molinos.Orquest.Dominio.Comandos;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverPuestoDeVianda : IDriver
    {
        void NotificarLecturaViandas(NotificarLecturaViandas comando);
    }
}