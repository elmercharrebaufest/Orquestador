using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.Servidor
{
	public class ConfiguracionGeneral : IConfiguracionGeneral
	{
		public int TiempoReintentoReconexion { get; set; }
		public int TiempoPing { get; set; }
	}
}
