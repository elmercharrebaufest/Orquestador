using Molinos.Orquest.Dominio.Recursos;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigLectorQr")]
    public class ConfigLectorQr : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [StringLength(5, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public virtual string Lector { get; set; }
    }
}
