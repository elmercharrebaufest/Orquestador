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
    public class ProcesadorEjecutarAperturaBarreraMaestro : ProcesadorComando<EjecutarAperturaBarreraMaestro, ResultadoEjecutar>
    {
        public ProcesadorEjecutarAperturaBarreraMaestro(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarAperturaBarreraMaestro comando, Dispositivo dispositivo, IDriver driver)
        {
            ((IDriverBarrera)driver).AbrirMaestro();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
