using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigJsonFromIotBox")]
    public class ConfigJsonFromIotBox :ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "NumeroEntrada")]
        public virtual int NumeroEntrada { get; set; }
    }
}
