using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    //TODO: Deprecar
    public class ProcesadorEjecutarHabilitarMolinete : ProcesadorComando<EjecutarHabilitarTransito, ResultadoEjecutar>
    {
        public ProcesadorEjecutarHabilitarMolinete(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarHabilitarTransito ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverMolinete)driver).HabilitarTransito(ejecutar.Direccion, ejecutar.Tarjeta,ejecutar.FichadaId);
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
