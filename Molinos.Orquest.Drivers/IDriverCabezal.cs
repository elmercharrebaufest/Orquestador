namespace Molinos.Orquest.Drivers
{
    public interface IDriverCabezal : IDriver
    {
        decimal? ObtenerPeso();
        bool ForzarCero();
    }
}
