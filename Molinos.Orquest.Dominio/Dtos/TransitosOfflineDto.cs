using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Molinos.ControlDeAcceso.Dominio.DTOs
{
    public class TransitosOfflineDto
    {
        public int Id { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Nombre { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Apellido { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Dni { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string NroTarjeta { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int Sentido { get; set; }
        public string Molinete_id { get; set; }
        public int Puesto_id { get; set; }
        public string Puesto { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Hash { get; set; }
        public DateTime? FechaProcesado { get; set; }
    }
}
