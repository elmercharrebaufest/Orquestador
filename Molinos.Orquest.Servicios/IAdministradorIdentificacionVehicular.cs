using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System.Collections.Generic;

namespace Molinos.Orquest.Servicios
{
    public interface IAdministradorIdentificacionVehicular
    {
        void Iniciar(int idOrquestador);
        void Detener();
        void RecargarConfig(string codigoCIV);
        void ConectarDriver(string codigoDispositivo, IDriver driver);
        void DesconectarDriver(string codigoDispositivo);
        ResultadoSuscribir Suscribir(string codigoCIV, string codigoEvento, string rutaAccesoSuscriptor);
        ResultadoComando CancelarSuscripcion(string codigoCIV, string codigoEvento, string rutaAccesoSuscriptor);
        IList<EstadoDispositivoCIVDto> ObtenerEstadoDispositivosCIV(string codigoCIV);
        IList<NotificacionCIVDto> ObtenerUltimasNotificacionesCIV(string codigoCIV);
        void RegistrarNotificacionExterna(string codigoCIV, NotificacionCIVDto notificacion);
    }
}
