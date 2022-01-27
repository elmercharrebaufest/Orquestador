using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarImpresionTicket : ProcesadorComando<EjecutarImpresionTicket, ResultadoEjecutar>
    {
        private readonly ILogger log;

        public ProcesadorEjecutarImpresionTicket(ILogger log) : base(log)
        {
            this.log = log;
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarImpresionTicket comando, Dispositivo dispositivo, IDriver driver)
        {
            log.Debug($"Ejeutando impresion en {comando.CodigoDispositivo}");
            ((IDriverImpresoraHasar)driver).ImprimirTicket(comando);

            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
