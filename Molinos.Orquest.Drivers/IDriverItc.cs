namespace Molinos.Orquest.Drivers
{
    public interface IDriverItc : IDriver
    {
        void ActivarSalida(int salida, string estado, string dato, bool flush = false);
        void DesactivarSalida(int salida, string estado, string dato);
        bool ConsultarEstadoEntrada(int numeroEntrada);
        bool ConsultarEstadoActual(int numeroEntrada);
        void NotificarEstadoActual(int numeroEntrada);
    }
}
