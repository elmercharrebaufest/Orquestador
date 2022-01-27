using System;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ServicioSuscriptor : IServicioSuscriptor
    {
        public void Recibir(NotificacionEvento notificacion)
        {
            throw new NotImplementedException("Servicio dummy solo para publicar el WSDL del contrato");
        }
    }
}
