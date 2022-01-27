using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigMeteorologica")]
    public class ConfigMeteorologica : ConfigDispositivo
    {
        [Required]
        [Display(ResourceType = typeof(Textos), Name = "Ruta")]
        [StringLength(500, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string Ruta { get; set; }

    }
}
