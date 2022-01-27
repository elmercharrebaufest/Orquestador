using System.ServiceModel;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Servicios
{
    [ServiceContract(Namespace = "http://orquestador.molinos.com.ar")]
    public interface IServicioOrquestadorSAP
    {
        [OperationContract]
        ResultadoOperacion EjecutarPesaje(string codigoDispositivo);

        [OperationContract]
        ResultadoOperacion EjecutarCereoCabezal(string codigoDispositivo);
    }
}
