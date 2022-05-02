using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("FormatosJson")]
    public class FormatosJson
    {
        public int Id { get; set; }
        public string Formato { get; set; }
        public string Dispositivo_nombre { get; set; }
    }
}
