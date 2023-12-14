namespace Molinos.Orquest.Dominio.Dtos
{
    public class MensajeIntervaloDto
    {
        public string NumeroPrograma { get; set; }
        public string NumeroTrama { get; set; }
        public string NumeroVariable { get; set; }
        public int Intervalo { get; set; }
        public string MensajeActual { get; set; }
    }
}