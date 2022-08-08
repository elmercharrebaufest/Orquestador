using System.ComponentModel.DataAnnotations.Schema;

namespace Molinos.Orquest.Dominio.Entidades
{
    [Table("ConfigGrupoBarrera")]
    public class ConfigGrupoBarrera : ConfigDispositivo
    {
        [Column("BarreraArriba_Id")]
        public virtual int BarreraArribaId { get; set; }

        [Column("BarreraAbajo_Id")]
        public virtual int BarreraAbajoId { get; set; }

        [Column("SensorArriba_Id")]
        public virtual int SensorArribaId { get; set; }

        [Column("SensorAbajo_Id")]
        public virtual int SensorAbajoId { get; set; }

        [Column("SensorPrimerCruce_Id")]
        public virtual int SensorPrimerCruceId { get; set; }

        [Column("SensorSegundoCruce_Id")]
        public virtual int? SensorSegundoCruceId { get; set; }

        public virtual ConfigBarrera BarreraArriba { get; set; }
        public virtual ConfigBarrera BarreraAbajo { get; set; }
        public virtual ConfigSensor SensorArriba { get; set; }
        public virtual ConfigSensor SensorAbajo { get; set; }
        public virtual ConfigSensor SensorPrimerCruce { get; set; }
        public virtual ConfigSensor SensorSegundoCruce { get; set; }
    }
}