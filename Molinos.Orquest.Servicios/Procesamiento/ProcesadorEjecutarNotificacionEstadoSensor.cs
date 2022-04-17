using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarNotificacionEstadoSensor : ProcesadorComando<EjecutarNotificacionEstadoSensor, ResultadoEjecutar>
    {
        public ProcesadorEjecutarNotificacionEstadoSensor(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarNotificacionEstadoSensor comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("Comando EjecutarNotificacionEstadoSensor : " + comando.ToString());
            ((IDriverSensor)driver).NotificarEstadoActualSensor();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
