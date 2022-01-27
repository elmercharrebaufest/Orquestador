using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarObtenerDireccionViento : ProcesadorComando<EjecutarObtenerDireccionViento, ResultadoEjecutar>
    {
        public ProcesadorEjecutarObtenerDireccionViento(ILogger log): base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarObtenerDireccionViento ejecutar, Dispositivo dispositivo, IDriver driver)
        {            
            var veleta = ((IDriverMeteorologica) driver).ObtenerDireccionViento();

            return veleta < 0
                       ? new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Viento", veleta)
                       : new ResultadoEjecutar
                       {
                           //Mesnaje de error retornar -1
                           Mensaje = new Mensaje(Codigos.LecturaEscritura, Textos.Error_ObtenerDireccionViento, ejecutar.CodigoDispositivo)
                       };
           
        }
    }
}
