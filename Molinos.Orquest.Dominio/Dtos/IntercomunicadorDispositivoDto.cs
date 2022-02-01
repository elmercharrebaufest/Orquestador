namespace Molinos.Orquest.Dominio.Dtos
{
    public class IntercomunicadorDispositivoDto
    {
        public string UniqueId { get; set; }
        public string Codigo { get; set; }
        public string AudioPort { get; set; }
        public string ICPCConfig { get; set; }
        public string ICWebServerUrl { get; set; }
        public string ICWSServerUrl { get; set; }
        public string DeviceActivationUrl { get; set; }
        public string PublishingPathListen { get; set; }
        public string PublishingPathSpeak { get; set; }
    }
}