using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarAnalisis : ProcesadorComando<EjecutarAnalisis, ResultadoEjecutar>
    {
        public ProcesadorEjecutarAnalisis(ILogger log)
            : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarAnalisis comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("logComando 1 {0}", comando);
            Log.Info("logComando 2 {0}", dispositivo);

            var analisis = ((IDriverNirs) driver).ObtenerAnalisis(comando.Material, comando.IdentificadorDeMuestra);
            return analisis != null ? 
                new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar(analisis) :
                new ResultadoEjecutar 
                {
                    Mensaje = new Mensaje(Codigos.SinLecturaDeHumedad, Textos.ResultadoSinLecturaDeHumedad, comando.CodigoDispositivo)
                };
        }
    }
}
