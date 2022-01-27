using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Web.Models
{
    public class ConfigPuestoDeViandaModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "PuestoDeVianda_Sector")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public string Sector { get; set; }
        [Display(ResourceType = typeof(Textos), Name = "PuestoDeVianda_Pantallas")]
        public ICollection<ListaPantallaPuestoDeVianda> ConfigPantallaPuestoDeViandas { get; set; }

    }
}