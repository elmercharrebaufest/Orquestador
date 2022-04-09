using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverSensor : IDriver
    {
        ResultadoEstadoSensor ConsultaEstadoActual();
    }
}
