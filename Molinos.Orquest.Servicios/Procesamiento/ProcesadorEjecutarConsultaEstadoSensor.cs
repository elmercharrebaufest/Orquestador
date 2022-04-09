using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarConsultaEstadoSensor : ProcesadorComando<EjecutarConsultaEstadoSensor, ResultadoEjecutar>
    {
        public ProcesadorEjecutarConsultaEstadoSensor(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarConsultaEstadoSensor comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("Comando ConsultaEstadoSensor : " + comando.ToString());
            return ((IDriverSensor)driver).ConsultaEstadoActual();
        }
    }
}
