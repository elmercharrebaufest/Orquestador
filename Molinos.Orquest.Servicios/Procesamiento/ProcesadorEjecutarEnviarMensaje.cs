using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarEnviarMensaje : ProcesadorComando<EjecutarEnviarMensaje, ResultadoEjecutar>
    {
            public ProcesadorEjecutarEnviarMensaje(ILogger log) : base(log)
            {
            }

            protected override ResultadoEjecutar Ejecutar(EjecutarEnviarMensaje comando, Dispositivo dispositivo, IDriver driver)
            {
                ((IDriverCartelLed) driver).EnviarMensaje(comando.Texto, comando.NumeroPrograma, comando.NumeroTrama, comando.NumeroVariable);
                return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
            }
    }
}
