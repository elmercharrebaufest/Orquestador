using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class ConsultaMolinete
    {
        public NuevaTarjeta NuevaTarjeta { get; set; }
        public NuevoTransito NuevoTransito { get; set; }
        public NuevoQR NuevoQR { get; set; }
        public PulsadorEmergencia PulsadorEmergencia { get; set; }
    }

    public class NuevaTarjeta
    {
        public string Tarjeta { get; set; }
        public string Lector { get; set; }
    }
    public class NuevoTransito
    {
        public string Direccion { get; set; }
        public bool Denegado { get; set; }
        public bool SinTransito { get; set; }
    }
    public class NuevoQR
    {
        public string QR { get; set; }
        public string Lector { get; set; }
    }
    public class PulsadorEmergencia
    {
        public string Estado { get; set; }
    }
}