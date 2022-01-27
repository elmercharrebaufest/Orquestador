using System.Collections.Generic;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverBalanzaPuerto : IDriver
    {
        bool BorrarBalanzada(int IdBalanzada);

        Dictionary<string, bool> BorrarBalanzadasPorRango(int IdBalanzadaInicio, int IdBalanzadaFin);
        Dictionary<string,string> ConsultaBalanzada(int? IdBalanzada);
    }
}
