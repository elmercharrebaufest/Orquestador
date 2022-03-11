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
    public class ProcesadorEjecutarAperturaBarrera : ProcesadorComando<EjecutarAperturaBarrera, ResultadoEjecutar>
    {
        public ProcesadorEjecutarAperturaBarrera(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarAperturaBarrera comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("Comando Apertura Barrera Garita : " + comando.ToString());
            ((IDriverBarrera)driver).Abrir();
            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
