namespace Molinos.Orquest.Drivers
{
	public interface IConfiguracionGeneral
	{
		int TiempoReintentoReconexion { get; }
		int TiempoPing { get; }
	}
}
