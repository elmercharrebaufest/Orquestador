using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigJsonToIotBox")]

    public class ConfigJsonToIotBox : ConfigDispositivo
    {
        virtual public int NumeroSalida { get; set; }
        virtual public int? FormatoJson { get; set; }
    }
}
