using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    public abstract class ConfigDispositivo
    {
        [Key, ForeignKey("Dispositivo")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public virtual int Id { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "Cabezal_ClaseDriver")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual string ClaseDriver { get; set; }
        public virtual Dispositivo Dispositivo { get; set; }
    }
}
