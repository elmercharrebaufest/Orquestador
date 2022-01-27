using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarAperturaCortinaAgua : ProcesadorComando<EjecutarAperturaCortinaAgua, ResultadoEjecutar>
    {
        public ProcesadorEjecutarAperturaCortinaAgua(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarAperturaCortinaAgua comando, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverCortinaAgua)driver).Abrir();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
