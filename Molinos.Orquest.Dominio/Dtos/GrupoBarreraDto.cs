namespace Molinos.Orquest.Dominio.Dtos
{
    public class GrupoBarreraDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public int BarreraArribaId { get; set; }
        public DispositivoDto BarreraArriba { get; set; }
        public int BarreraAbajoId { get; set; }
        public DispositivoDto BarreraAbajo { get; set; }
        public int SensorArribaId { get; set; }
        public DispositivoDto SensorArriba { get; set; }
        public int SensorAbajoId { get; set; }
        public DispositivoDto SensorAbajo { get; set; }
        public int SensorPrimerCruceId { get; set; }
        public DispositivoDto SensorPrimerCruce { get; set; }
        public int SensorSegundoCruceId { get; set; }
        public DispositivoDto SensorSegundoCruce { get; set; }
        public string ClaseDriver { get; set; }
        public bool Activo { get; set; }
    }
}