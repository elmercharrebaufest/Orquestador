using Molinos.Orquest.Dominio.Resultados;
using System.Collections.Generic;

namespace Molinos.Orquest.Web.ServicioHub
{
    /// <summary>
    /// Payload enviado por SignalR al Monitor de Identificación Vehicular
    /// cuando se recibe un nuevo evento IdentificacionVehicular.
    /// </summary>
    public class NotificacionCIV
    {
        /// <summary>Código de la ConfigIdentificacionVehicular (para routing al grupo hub).</summary>
        public string CodigoCIV { get; set; }

        public string CodigoEvento { get; set; }

        public string CodigoDispositivo { get; set; }

        /// <summary>Código de tarjeta leído (vacío si el trigger fue el sensor vehicular).</summary>
        public string Valor { get; set; }

        public bool VehiculoPresente { get; set; }

        public string Patente { get; set; }

        public string FechaEvento { get; set; }

        public IList<DetalleCamaraCIV> Detalles { get; set; }

        /// <summary>JSON serializado de la NotificacionEvento original para el panel de diagnóstico.</summary>
        public string JsonCompleto { get; set; }
    }
}
