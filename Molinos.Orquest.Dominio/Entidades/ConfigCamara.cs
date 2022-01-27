using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigCamara")]
    public class ConfigCamara : ConfigDispositivo
    {
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Camara_Uri")]
        [StringLength(100, ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_ExcedeLargoMaximo")]
        public string Uri { get; set; }

        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        [Display(ResourceType = typeof(Textos), Name = "Camara_TimeoutLectura")]
        public virtual int TimeoutLectura { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Camara_Margen_Derecho")]
        public virtual int? MargenDerecho { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Camara_Margen_Izquierdo")]
        public virtual int? MargenIzquierdo { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Camara_Margen_Superior")]
        public virtual int? MargenSuperior { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Camara_Margen_Inferior")]
        public virtual int? MargenInferior { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Camara_Rotacion")]
        public RotateFlipType? RotateFlipType { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Nombre_Usuario")]
        public string NombreUsuario { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "Contrasenia")]
        public virtual string Contrasenia { get; set; }

        [Display(ResourceType = typeof(Textos), Name = "UrlStreaming")]
        public virtual string UrlStreaming { get; set; }

        public virtual string DireccionIp { get; set; }

        public virtual int? Puerto { get; set; }

        public virtual int? LongFrase { get; set; }
    }
}
