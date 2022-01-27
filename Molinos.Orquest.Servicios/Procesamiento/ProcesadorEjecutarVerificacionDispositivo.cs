using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarVerificacionDispositivo : ProcesadorComando<EjecutarVerificacionDispositivo, ResultadoEjecutar>
    {
        private ILogger logg;
        public ProcesadorEjecutarVerificacionDispositivo(ILogger log) : base(log)
        {
            logg = log;
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarVerificacionDispositivo comando, Dispositivo dispositivo, IDriver driver)
        {
            logg.Debug("Tipo Driver: " + (driver.TipoDispositivo != null ? driver.TipoDispositivo.ToString() : string.Empty));
            driver.VerificarDispositivo();
            return new ResultadoEjecutar {Mensaje = Mensaje.ResultadoOK()};
        }
    }
}
