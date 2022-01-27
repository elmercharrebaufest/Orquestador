using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarBorrarBalanzada : ProcesadorComando<EjecutarBorrarBalanzada, ResultadoEjecutar>
    {
        public ProcesadorEjecutarBorrarBalanzada(ILogger log): base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarBorrarBalanzada ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            var IdBalanzada = ((IDriverBalanzaPuerto) driver).BorrarBalanzada(ejecutar.IdBorrado);
            return new ResultadoBorrarBalanzada
                {
                    Mensaje = Mensaje.ResultadoOK()
                };
        }
    }
}
