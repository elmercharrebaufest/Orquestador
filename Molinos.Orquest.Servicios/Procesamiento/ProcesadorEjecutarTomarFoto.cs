using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarTomarFoto : ProcesadorComando<EjecutarTomarFoto, ResultadoEjecutar>
    {
        public ProcesadorEjecutarTomarFoto(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarTomarFoto ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            var resultado = ((IDriverCamara)driver).TomarFoto(ejecutar.FilePath, ejecutar.SubPath, ejecutar.FileName);
            return resultado;
        }
    }
}
