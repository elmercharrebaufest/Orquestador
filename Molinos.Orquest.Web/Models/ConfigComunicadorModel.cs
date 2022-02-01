using Molinos.Orquest.Dominio.Entidades;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigComunicador")]
    public class ConfigComunicadorModel : ConfigDispositivo
    {
        public int NumeroSalida { get; set; }
        public int? TiempoMaximoEjecucion { get; set; }
        public int? PuertoDeAudio { get; set; }
    }
}