using System.Collections.Generic;
using System.ServiceModel;
using Molinos.Orquest.Dominio.Dtos;

namespace Molinos.Orquest.Web
{
    [ServiceContract(Namespace = "http://dispositivos.orquestador.molinos.com.ar")]
    public interface IServicioDispositivos
    {
        [OperationContract]
        IList<DispositivoDto> ListarLectores();

        [OperationContract]
        IList<DispositivoDto> ListarBalanzas();

        [OperationContract]
        IList<DispositivoDto> ListarBarrerasSemaforos();
    }
}
