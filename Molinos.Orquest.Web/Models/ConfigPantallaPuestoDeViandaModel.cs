using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Molinos.Orquest.Web.Models
{
    public class ConfigPantallaPuestoDeViandaModel : ConfigDispositivo
    {
        [Display(ResourceType = typeof(Textos), Name = "PuestoDeVianda")]
        [Required(ErrorMessageResourceType = typeof(Textos), ErrorMessageResourceName = "Error_Requerido")]
        public virtual int ConfigPuestoDeViandas { get; set; }
    }
    public class ListaPantallaPuestoDeVianda : ConfigDispositivo
    {
        public virtual ConfigPuestoDeVianda ConfigPuestoDeVianda { get; set; }
    }
}