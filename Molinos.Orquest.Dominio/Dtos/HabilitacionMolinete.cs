using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class HabilitarTransito
    {
        public string Direccion { get; set; }
        public int Timeout { get; set; }
    }
    public class HabilitacionMolinete
    {
        public HabilitarTransito HabilitarTransito { get; set; }
    }
}