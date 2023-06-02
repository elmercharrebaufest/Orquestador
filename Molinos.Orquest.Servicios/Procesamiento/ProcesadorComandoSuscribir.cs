using System.Linq;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorComandoSuscribir : ProcesadorComando<ComandoSuscribir, ResultadoSuscribir>
    {
        private readonly IAdministradorSuscripciones adminSuscripciones;

        public ProcesadorComandoSuscribir(IAdministradorSuscripciones adminSuscripciones, ILogger log) : base(log)
        {
            this.adminSuscripciones = adminSuscripciones;
        }

        protected override ResultadoSuscribir Ejecutar(ComandoSuscribir comando, Dispositivo dispositivo, IDriver driver)
        {
            Log.Info("--OSCAR LecturaTarjetaRecibida 3. " + comando.CodigoEvento);
            ResultadoSuscribir resultado;
            if (driver.EventosSoportados.Any(evento => evento == comando.CodigoEvento))
            {
                var idSuscripcion = adminSuscripciones.CrearSuscripcion(dispositivo.Id, comando.CodigoEvento, comando.RutaAccesoSuscriptor, comando.Persistente);
                Log.Info("--OSCAR LecturaTarjetaRecibida 4.");
                resultado = new ResultadoSuscribir
                    {
                        Mensaje = Mensaje.ResultadoOK(),
                        IdSuscripcion = idSuscripcion
                    };
                if (comando.CodigoEvento == CodigosEventos.ConexionDispositivoCorrecta ||
                    comando.CodigoEvento == CodigosEventos.ErrorConexionDispositivo)
                {
                    Log.Info("--OSCAR LecturaTarjetaRecibida 5.");
                    driver.InformarEstado();
                }
            }
            else
            {
                Log.Info("--OSCAR LecturaTarjetaRecibida 6.");
                resultado = new ResultadoSuscribir
                    {
                        Mensaje = new Mensaje(Codigos.EventoNoSoportado, Textos.ResultadoEventoNoSoportadoPorDriver,
                            dispositivo.Configuracion.ClaseDriver,
                            dispositivo.Codigo,
                            comando.CodigoEvento)
                    };
            }
            return resultado;
        }
    }
}
