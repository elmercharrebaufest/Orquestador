namespace Molinos.Orquest.Dominio.Dtos
{
    public class ConfigIdentificacionVehicularDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string CodigoLectorTarjetas { get; set; }
        public string CodigoSensorVehicular { get; set; }
        public string CodigoSensorPresencia { get; set; }
        public bool Activo { get; set; }
    }
}
