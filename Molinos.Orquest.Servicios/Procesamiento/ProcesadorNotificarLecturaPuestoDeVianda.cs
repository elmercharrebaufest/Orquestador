using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorNotificarLecturaPuestoDeVianda : ProcesadorComando<NotificarLecturaViandas, ResultadoEjecutar>
    {
        public ProcesadorNotificarLecturaPuestoDeVianda(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(NotificarLecturaViandas ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverPuestoDeVianda)driver).NotificarLecturaViandas(ejecutar);
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
