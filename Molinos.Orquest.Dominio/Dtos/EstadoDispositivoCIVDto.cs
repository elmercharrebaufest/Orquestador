using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Dtos
{
    [DataContract]
    public class EstadoDispositivoCIVDto
    {
        [DataMember]
        public string CodigoDispositivo { get; set; }

        /// <summary>"LectorTarjetas" | "SensorVehicular" | "SensorPresencia" | "Camara"</summary>
        [DataMember]
        public string TipoDispositivo { get; set; }

        [DataMember]
        public bool Conectado { get; set; }

        /// <summary>Solo para LectorTarjetas: último valor de tarjeta leído.</summary>
        [DataMember]
        public string UltimoValor { get; set; }

        /// <summary>Solo para SensorPresencia: último estado conocido.</summary>
        [DataMember]
        public bool? VehiculoPresente { get; set; }

        /// <summary>Solo para Camara: última patente detectada (null si aún no hubo captura).</summary>
        [DataMember]
        public string UltimaPatente { get; set; }
    }
}
