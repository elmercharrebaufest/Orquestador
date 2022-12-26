using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorDetenerMensajeIntervalo : ProcesadorComando<DetenerMensajeIntervalo, ResultadoEjecutar>
    {
        public ProcesadorDetenerMensajeIntervalo(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(DetenerMensajeIntervalo comando, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverCartelLed)driver).DetenerIntervalo();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}