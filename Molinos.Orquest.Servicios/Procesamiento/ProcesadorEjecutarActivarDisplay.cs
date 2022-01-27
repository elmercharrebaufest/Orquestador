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
    public class ProcesadorEjecutarActivarDisplay : ProcesadorComando<EjecutarActivarDisplay, ResultadoEjecutar>
    {
        public ProcesadorEjecutarActivarDisplay(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarActivarDisplay comando, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverDisplay)driver).Abrir();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
