using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarPesaje : ProcesadorComando<EjecutarPesaje, ResultadoEjecutar>
    {
        public ProcesadorEjecutarPesaje(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarPesaje comando, Dispositivo dispositivo, IDriver driver)
        {
            var peso = ((IDriverCabezal)driver).ObtenerPeso();
            return peso.HasValue
                       ? new ResultadoEjecutar {Mensaje = Mensaje.ResultadoOK()}.Agregar("Pesaje", peso.Value)
                       : new ResultadoEjecutar
                           {
                               Mensaje =
                                   new Mensaje(Codigos.CabezalPesoNoEstable, Textos.ResultadoPesoNoEstable,
                                               comando.CodigoDispositivo)
                           };
        }
    }
}
