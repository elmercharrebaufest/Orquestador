using Molinos.Orquest.Dominio.Resultados;
using System.ServiceModel;

namespace Molinos.Orquest.ModuloALPR
{
    [ServiceContract(Namespace = "http://orquestador.molinos.com.ar")]
    public interface IServicioALPR
    {
        [OperationContract]
        ResultadoObtenerPatente LeerPatente(byte[] imagen, int margenIzquierdo, int margenDerecho, int margenSuperior, int margenInferior);
    }
}
