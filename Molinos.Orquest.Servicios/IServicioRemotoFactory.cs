using Molinos.Orquest.Servicios.Impl;

namespace Molinos.Orquest.Servicios
{
    public interface IServicioRemotoFactory
    {
        ClienteServicio<IServicioOrquestador> CrearServicioOrquestador(string rutaServicio);
        ClienteServicio<IServicioSuscriptor> CrearServicioSuscriptor(string rutaServicio);
    }
}
