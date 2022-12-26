using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarEnviarMensajeIntervalo : ProcesadorComando<EjecutarEnviarMensajeIntervalo, ResultadoEjecutar>
    {
        public ProcesadorEjecutarEnviarMensajeIntervalo(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarEnviarMensajeIntervalo comando, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverCartelLed)driver).EnviarMensajeIntervalo(comando.Texto, comando.TextSecundario, comando.NumeroPrograma, comando.NumeroTrama, comando.NumeroVariable, comando.Intervalo);
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}