using Molinos.Orquest.Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Dtos
{
    [DataContract]
    public class NotificacionCIVDto
    {
        [DataMember]
        public string CodigoEvento { get; set; }

        [DataMember]
        public string CodigoDispositivo { get; set; }

        /// <summary>Código de tarjeta leído (vacío si el trigger fue el sensor vehicular).</summary>
        [DataMember]
        public string Valor { get; set; }

        [DataMember]
        public bool VehiculoPresente { get; set; }

        [DataMember]
        public string Patente { get; set; }

        [DataMember]
        public DateTime FechaEvento { get; set; }

        [DataMember]
        public IList<DetalleCamaraCIV> Detalles { get; set; }

        /// <summary>JSON serializado de la NotificacionEvento original (para el panel "Última Notificación").</summary>
        [DataMember]
        public string JsonCompleto { get; set; }
    }
}
