using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigComunicador")]
    public class ConfigComunicadorModel : ConfigDispositivo
    {
      
       public int NumeroSalida { get; set; }
       public int? TiempoMaximoEjecucion { get; set; }
    }
}