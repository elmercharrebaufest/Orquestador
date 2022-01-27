using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarEstacionMeteorologica : ProcesadorComando<EjecutarEstacionMeteorologica, ResultadoEjecutar>
    {
        public ProcesadorEjecutarEstacionMeteorologica(ILogger log): base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarEstacionMeteorologica ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            var imagenes = ((IDriverMeteorologica) driver).ObtenerGraficosMeteorologicos();
            return new ResultadoMeteorologica
                {
                    Imagenes = imagenes,
                    Mensaje = Mensaje.ResultadoOK()
                };
        }
    }
}
