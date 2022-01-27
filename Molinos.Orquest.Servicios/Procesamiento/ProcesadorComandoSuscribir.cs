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
            ResultadoSuscribir resultado;
            if (driver.EventosSoportados.Any(evento => evento == comando.CodigoEvento))
            {
                var idSuscripcion = adminSuscripciones.CrearSuscripcion(dispositivo.Id, comando.CodigoEvento, comando.RutaAccesoSuscriptor, comando.Persistente);
                resultado = new ResultadoSuscribir
                    {
                        Mensaje = Mensaje.ResultadoOK(),
                        IdSuscripcion = idSuscripcion
                    };
                if (comando.CodigoEvento == CodigosEventos.ConexionDispositivoCorrecta ||
                    comando.CodigoEvento == CodigosEventos.ErrorConexionDispositivo)
                {
                    driver.InformarEstado();
                }
            }
            else
            {
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
