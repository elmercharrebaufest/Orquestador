using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web.Models
{
    [Table("ConfigTagModel")]
    public class ConfigTagModel : ConfigDispositivo
    {
       public string Query { get; set; }
       public int Timeout { get; set; }
       //public int Historian { get; set; }
    }
}