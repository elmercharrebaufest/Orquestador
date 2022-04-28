using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarConsultaSensor : ProcesadorComando<EjecutarConsultaSensor, ResultadoEstadoSensor>
    {
        public ProcesadorEjecutarConsultaSensor(ILogger log) : base(log)
        {
        }

        protected override ResultadoEstadoSensor Ejecutar(EjecutarConsultaSensor comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("Comando ConsultaEstadoSensor : " + comando.ToString());
            return ((IDriverSensor)driver).ConsultaEstadoActual();
        }
    }
}
