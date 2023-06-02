using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorComandoCancelarSuscripcion : ProcesadorComando<ComandoCancelarSuscripcion, ResultadoCancelarSuscripcion>
    {
        private readonly IAdministradorSuscripciones adminSuscripciones;

        public ProcesadorComandoCancelarSuscripcion(IAdministradorSuscripciones adminSuscripciones, ILogger log) : base(log)
        {
            this.adminSuscripciones = adminSuscripciones;
        }

        protected override ResultadoCancelarSuscripcion Ejecutar(ComandoCancelarSuscripcion comando, Dispositivo dispositivo, IDriver driver)
        {
            try
            {
                if (comando.IdSuscripcion > 0)
                {
                    Log.Info("--OSCAR LecturaTarjetaRecibida 30");
                    adminSuscripciones.CancelarSuscripcion(comando.IdSuscripcion, dispositivo.Id, comando.CancelarTodas);
                }
                else
                {
                    adminSuscripciones.CancelarSuscripcionPorRutaAcceso(comando.RutaAccesoSuscriptor, dispositivo.Id, comando.CancelarTodas);
                }
                

                return new ResultadoCancelarSuscripcion
                    {
                        Mensaje = Mensaje.ResultadoOK()
                    };
            }
            catch (SuscripcionNoEncontradaException)
            {
                Log.Warn("No existe una suscripción con Id={0} para el dispositivo {1}", comando.IdSuscripcion, comando.CodigoDispositivo);
                return new ResultadoCancelarSuscripcion
                    {
                        Mensaje =
                            new Mensaje(Codigos.SuscripcionInexistente, Textos.ResultadoSuscripcionInexistente,
                                        comando.IdSuscripcion, comando.CodigoDispositivo)
                    };
            }
        }
    }
}
