using System;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Servicios
{
    public interface IAdministradorSuscripciones
    {
        int CrearSuscripcion(int idDispositivo, string codigoEvento, string rutaAccesoSuscriptor, bool persistente);
        void CancelarSuscripcion(int idSuscripcion, int idDispositivo, bool cancelarTodas);
        void CancelarSuscripcionPorRutaAcceso(string rutaAccesoSuscriptor, int idDispositivo, bool cancelarTodas);
        void Notificar(NotificacionEvento evento);
        bool ExistenSuscripcionesPara(int idDispositivo);
        void DepurarSuscripciones(int idDispositivo, DateTime vencimiento, bool depurarPersistentes);
    }
}
