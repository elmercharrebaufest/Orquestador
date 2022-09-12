namespace Molinos.Orquest.Dominio.Dtos
{
    public class GrupoBarreraDto
    {
        public int Id { get; set; }
        public string AgrupadorCodigo { get; set; }
        public string SensorPrimerCruceCodigo { get; set; }
        public string SensorSegundoCruceCodigo { get; set; }
        public string SensorArribaCodigo { get; set; }
        public string SensorAbajoCodigo { get; set; }
        public string BarreraArribaCodigo { get; set; }
        public string BarreraAbajoCodigo { get; set; }
        public string AgrupadorClaseDriver { get; set; }
    }
}