using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarBorrarBalanzadasPorRango : ProcesadorComando<EjecutarBorrarBalanzadasPorRango, ResultadoEjecutar>
    {
        public ProcesadorEjecutarBorrarBalanzadasPorRango(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarBorrarBalanzadasPorRango ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            if (ValidarRango(ejecutar.IdBalanzadaInicio, ejecutar.IdBalanzadaFin))
            {
                var balanzadasBorradasOk = ((IDriverBalanzaPuerto)driver).BorrarBalanzadasPorRango(ejecutar.IdBalanzadaInicio, ejecutar.IdBalanzadaFin);
                return new ResultadoBorrarBalanzadasPorRango
                {
                    IdBalanzadaInicio = ejecutar.IdBalanzadaInicio,
                    IdBalanzadaFin = ejecutar.IdBalanzadaFin,
                    BalanzadasBorradas = balanzadasBorradasOk,
                    Mensaje = Mensaje.ResultadoOK()
                };

            }
            else
            {
                return new ResultadoEjecutar
                {
                    Mensaje = Mensaje.ResultadoOK()
                };
            }

        }

        private bool ValidarRango(int inicio, int fin)
        {
            try
            {
                return inicio <= fin;
            }
            catch
            {
                throw new System.Exception("Error en los rangos.");
            }
        }
    }
}
