using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarObtenerUltimaFoto : ProcesadorComando<EjecutarObtenerUltimaFoto, ResultadoEjecutar>
    {
        public ProcesadorEjecutarObtenerUltimaFoto(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarObtenerUltimaFoto ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            var resultado = ((IDriverPantalla)driver).ObtenerUltimaFoto();
            return resultado;
        }
    }
}
