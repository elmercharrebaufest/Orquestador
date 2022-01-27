using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarCereoCabezal : ProcesadorComando<EjecutarCereoCabezal, ResultadoEjecutar>
    {
            public ProcesadorEjecutarCereoCabezal(ILogger log) : base(log)
            {
            }

            protected override ResultadoEjecutar Ejecutar(EjecutarCereoCabezal comando, Dispositivo dispositivo, IDriver driver)
            {
                var enCero = ((IDriverCabezal) driver).ForzarCero();
                return enCero
                           ? new ResultadoEjecutar {Mensaje = Mensaje.ResultadoOK()}
                           : new ResultadoEjecutar
                               {
                                   Mensaje = new Mensaje(
                                        Codigos.CabezalNoFuerzaCero,
                                        Textos.ResultadoCabezalNoFuerzaCero,
                                        comando.CodigoDispositivo 
                                       )
                               };
            }
    }
}
