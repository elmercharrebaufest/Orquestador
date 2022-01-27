using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarConsultaBalanzada : ProcesadorComando<EjecutarConsultaBalanzada, ResultadoEjecutar>
    {
        public ProcesadorEjecutarConsultaBalanzada(ILogger log): base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarConsultaBalanzada ejecutar, Dispositivo dispositivo, IDriver driver)
        {            
            var balanzada = ((IDriverBalanzaPuerto) driver).ConsultaBalanzada(ejecutar.IdBalanzada);
           
            return balanzada != null && balanzada.Count > 0
                       ? new ResultadoConsultaBalanzada { Mensaje = Mensaje.ResultadoOK() }.Agregar(balanzada)
                       : new ResultadoConsultaBalanzada
                       {
                           Mensaje = new Mensaje(Codigos.BalanzadaNoEncontrada, Textos.ResultadoBalanzadaNoEncontrada, ejecutar.IdBalanzada)
                       };
           
        }
    }
}
