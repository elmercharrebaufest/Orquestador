using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorComandoDepurarSuscripciones : ProcesadorComando<ComandoDepurarSuscripciones, ResultadoComando>
    {
        private readonly IAdministradorSuscripciones adminSuscripciones;

        public ProcesadorComandoDepurarSuscripciones(IAdministradorSuscripciones adminSuscripciones, ILogger log) : base(log)
        {
            this.adminSuscripciones = adminSuscripciones;
        }

        protected override ResultadoComando Ejecutar(ComandoDepurarSuscripciones comando, Dispositivo dispositivo, IDriver driver)
        {
            adminSuscripciones.DepurarSuscripciones(dispositivo.Id, comando.Vencimiento, comando.DepurarPersistentes);
            return new ResultadoCancelarSuscripcion
            {
                Mensaje = Mensaje.ResultadoOK()
            };
        }
    }
}
