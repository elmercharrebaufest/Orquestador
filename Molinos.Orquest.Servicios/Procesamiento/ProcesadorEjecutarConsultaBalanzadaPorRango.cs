using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;
using System.Collections.Generic;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarConsultaBalanzadaPorRango : ProcesadorComando<EjecutarConsultaBalanzadaPorRango, ResultadoEjecutar>
    {
        public ProcesadorEjecutarConsultaBalanzadaPorRango(ILogger log): base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarConsultaBalanzadaPorRango ejecutar, Dispositivo dispositivo, IDriver driver)
        {
            var balanzadas = new List<Dictionary<string, string>>();

            var resultados = new ResultadoConsultaBalanzadaPorRango();
            resultados.Balanzadas = new List<ResultadoConsultaBalanzada>(); 

            for (int i = ejecutar.IdBalanzadaInicio; i <= ejecutar.IdBalanzadaFin; i++)
            {
                var idBalanzadaActual = i;
                var balanzada = ((IDriverBalanzaPuerto)driver).ConsultaBalanzada(idBalanzadaActual);
                balanzadas.Add(balanzada);

                if (balanzada != null && balanzada.Count > 0)
                {
                    resultados.Balanzadas.Add(new ResultadoConsultaBalanzada { Mensaje = Mensaje.ResultadoOK() }.Agregar(balanzada));
                    
                }
                else
                {
                    resultados.Balanzadas.Add( new ResultadoConsultaBalanzada {
                        Mensaje = new Mensaje(Codigos.BalanzadaNoEncontrada, Textos.ResultadoBalanzadaNoEncontrada, idBalanzadaActual)
                    });
                }                
            }

            if (resultados.Balanzadas.TrueForAll(b => b.Mensaje.Codigo == Codigos.OK))
            {
                resultados.Mensaje = Mensaje.ResultadoOK();
            } else {
                resultados.Mensaje = new Mensaje(Codigos.BalanzadaNoEncontrada, Textos.ResultadoBalanzadaNoEncontrada, "en el rango");
            }
                    
            return resultados;
        }
    }
}
