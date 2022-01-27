using System.ServiceModel;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Web.ServicioHub
{
    [ServiceContract(Namespace = "http://suscriptor.orquestador.molinos.com.ar")]
    public interface IServicioSuscriptor
    {
        [OperationContract]
        void Recibir(NotificacionEvento notificacion);
    }
}
