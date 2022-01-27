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
    public class ProcesadorEjecutarEjecutarQuery : ProcesadorComando<EjecutarEjecutarQuery, ResultadoEjecutar>
    {
        public ProcesadorEjecutarEjecutarQuery(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarEjecutarQuery comando, Dispositivo dispositivo, IDriver driver)
        {
            var driverTag = (IDriverTag)driver;

            if (string.IsNullOrEmpty(comando.CustomQuery))
            {
                return driverTag.EjecutarQuery();
            }
            else
            {
                return driverTag.EjecutarQuery(comando.CustomQuery);
            }
        }

    }
}
