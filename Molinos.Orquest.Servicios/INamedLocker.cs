namespace Molinos.Orquest.Servicios
{
    public interface INamedLocker
    {
        object GetLock(string name);
    }
}
