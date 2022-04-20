using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Resultados;
using System.Collections.Generic;
using System.ServiceModel;

namespace Molinos.Orquest.Servicios
{
    [ServiceContract(Namespace = "http://orquestador.molinos.com.ar")]
    public interface IServicioOrquestador
    {
        [OperationContract]
        ResultadoEjecutar Ejecutar(ComandoEjecutar comando);

        [OperationContract]
        ResultadoSuscribir Suscribir(ComandoSuscribir comando);

        [OperationContract]
        ResultadoCancelarSuscripcion CancelarSuscripcion(ComandoCancelarSuscripcion comando);

        [OperationContract]
        ResultadoComando RecargarConfiguracion(string codigoDispositivo);

        [OperationContract]
        IList<DispositivoDto> ListarLectores();

        [OperationContract]
        IList<DispositivoDto> ListarPuestoDeViandas();

        [OperationContract]
        DispositivoDto ObtenerLectorPorPantalla(string codigoPantalla);

        [OperationContract]
        IList<DispositivoDto> ListarBalanzas();

        [OperationContract]
        IList<DispositivoDto> ListarBalanzasDePuerto();

        [OperationContract]
        IList<DispositivoDto> ListarBarrerasSemaforos();

        [OperationContract]
        IList<DispositivoDto> ListarSensores();

        [OperationContract]
        IList<DispositivoDto> ListarHumedimetros();

        [OperationContract]
        IList<DispositivoDto> ListarCamaras();

        [OperationContract]
        IList<DispositivoDto> ListarEstacionMeteorologica();

        [OperationContract]
        IList<DispositivoDto> ListarNirs();

        [OperationContract]
        IList<DispositivoDto> ListarImpresorasHasar();

        [OperationContract]
        IList<DispositivoDto> ListarLectoresQr();

        [OperationContract]
        IList<DispositivoDto> ListarCartelesLed();

        //TODO: Deprecar
        [OperationContract]
        IList<DispositivoDto> ListarMolinetes();

        [OperationContract]
        IList<DispositivoDto> ListarDisplays();

        [OperationContract]
        IList<DispositivoDto> ListarConcentradores();

        [OperationContract]
        IList<DispositivoDto> ListarSensoresPorConcentrador(string concentrador);

        void Iniciar(string nombreMaquina, string urlServicio);

        void VerificarDispositivos();

        void Detener();

        [OperationContract]
        void DetenerServiceOrquestador(string server);

        [OperationContract]
        string ObtenerEstadoServiceOrquestador(string server);

        [OperationContract]
        void IniciarServiceOrquestador(string server);

        [OperationContract]
        IList<string> ObtenerUrlPorCamara(string[] codigo);

        [OperationContract]
        IList<CamaraDto> ObtenerCamaras(string[] codigo);

        [OperationContract]
        IList<DispositivoDto> ListarIntercomunicadores();

        [OperationContract]
        void PrenderApagarDispositivo(string codigoDispositivo, bool activar, string server);

        [OperationContract]
        IntercomunicadorDispositivoBaseDto ObtenerIntercomunicadorPuertoDeAudio(string codigoDispositivo);

        [OperationContract]
        IList<DispositivoDto> ListarTags();
    }
}