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
    public class ProcesadorEjecutarEnviarJson : ProcesadorComando<EjecutarEnviarJson, ResultadoEjecutar>
    {
        public ProcesadorEjecutarEnviarJson(ILogger log) : base(log)
        {
        }
        protected override ResultadoEjecutar Ejecutar(EjecutarEnviarJson comando, Dispositivo dispositivo, IDriver driver)
        {
            var driverJson = (IDriverJsonToIotBox)driver;


            driverJson.EnviarJson(comando.json);
            return new ResultadoEjecutar
            {
                Mensaje = Mensaje.ResultadoOK()

            };

               // return driverTag.EjecutarQuery(comando.CustomQuery);

        }

    }
}
