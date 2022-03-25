using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarAnalisisHumedad : ProcesadorComando<EjecutarAnalisisHumedad, ResultadoEjecutar>
    {
        public ProcesadorEjecutarAnalisisHumedad(ILogger log)
            : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarAnalisisHumedad comando, Dispositivo dispositivo, IDriver driver)
        {
            if (comando.FechaDeInicio == null || comando.FechaDeInicio.Year == 1)
            {
                throw new ComandoDriverException();
            }
            var humedad = ((IDriverHumedimetro) driver).ObtenerHumedad(comando.FechaDeInicio);
            var PH = ((IDriverHumedimetro)driver).ObtenerPH(comando.FechaDeInicio);
            return humedad.HasValue || PH.HasValue ? 
                new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("AnalisisHumedad", humedad.Value).Agregar("PH",PH.Value):
                new ResultadoEjecutar 
                {
                    Mensaje = new Mensaje(Codigos.SinLecturaDeHumedad, Textos.ResultadoSinLecturaDeHumedad, comando.CodigoDispositivo)
                };
        }
    }
}
