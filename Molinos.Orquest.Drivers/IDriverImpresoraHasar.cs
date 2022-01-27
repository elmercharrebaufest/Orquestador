using Molinos.Orquest.Dominio.Comandos;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverImpresoraHasar : IDriver
    {
        void ImprimirTicket(EjecutarImpresionTicket comando);
    }
}