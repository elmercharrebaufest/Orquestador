using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Resultados;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ServicioOrquestadorSAP : IServicioOrquestadorSAP
    {
        private IServicioOrquestador servicioOrquestador;
        private ILogger log;

        public ServicioOrquestadorSAP(IServicioOrquestador servicioOrquestador, ILogger log)
        {
            this.servicioOrquestador = servicioOrquestador;
            this.log = log;
        }

        public ResultadoOperacion EjecutarPesaje(string codigoDispositivo)
        {
            var resultadoComando = servicioOrquestador.Ejecutar(new EjecutarPesaje {CodigoDispositivo = codigoDispositivo});
            return new ResultadoOperacion
                {
                    Codigo = resultadoComando.Mensaje.Codigo,
                    Descripcion = resultadoComando.Mensaje.Descripcion,
                    Valor = resultadoComando.Valores.ContainsKey("Pesaje") ? Convert.ToInt32(resultadoComando.Valores["Pesaje"]) : 0
                };
        }

        public ResultadoOperacion EjecutarCereoCabezal(string codigoDispositivo)
        {
            var resultadoComando = servicioOrquestador.Ejecutar(new EjecutarCereoCabezal { CodigoDispositivo = codigoDispositivo });
            return new ResultadoOperacion
            {
                Codigo = resultadoComando.Mensaje.Codigo,
                Descripcion = resultadoComando.Mensaje.Descripcion,
                Valor = 0
            };
        }
    }
}
