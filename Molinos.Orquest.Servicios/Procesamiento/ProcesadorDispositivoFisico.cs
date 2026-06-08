using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public sealed class ProcesadorDispositivoFisico : ProcesadorDispositivo
    {
        private readonly IAdministradorIdentificacionVehicular adminIdentificacion;

        public ProcesadorDispositivoFisico(Dispositivo dispositivo, IProcesadorFactory procesadorFactory, IDriverFactory driverFactory, IAdministradorSuscripciones adminSuscripciones, IAdministradorIdentificacionVehicular adminIdentificacion, Action<IProcesadorDispositivo> callBackFinProcesamiento, ILogger log) 
            : base(dispositivo, procesadorFactory, driverFactory, adminSuscripciones, callBackFinProcesamiento, log)
        {
            this.adminIdentificacion = adminIdentificacion;

            // Nos suscribimos a eventos del driver
            Driver.EventoDriver += (sender, args) =>
            {
                adminSuscripciones.Notificar(args.Notificacion);
                log.Debug($"Suscripcion Notificacion: Dispositivos: {args.Notificacion.CodigoDispositivo } Evento: {args.Notificacion.CodigoEvento}");
            };

            adminIdentificacion?.ConectarDriver(dispositivo.Codigo, Driver);
            
        }

        protected override ResultadoComando ProcesarComando(Comando comando)
        {
            if (comando.CodigoDispositivo != CodigoDispositivo)
            {
                throw new ProcesadorException(
                    string.Format("NO se puede procesar un comando para el dispositivo {0} en el procesador de {1}",
                                  comando.CodigoDispositivo, CodigoDispositivo));
            }

            Log.Debug("Obteniendo procesador para el comando {0}", comando);
            var procesadorComando = ProcesadorFactory.ProcesadorPara(comando);
            Log.Debug("Procesando comando {0}", comando);
            return procesadorComando.Procesar(comando, Dispositivo, Driver);
        }

        protected override bool PuedeLiberarProcesador()
        {
            return !Driver.MantenerConectado() && !AdminSuscripciones.ExistenSuscripcionesPara(Dispositivo.Id);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                adminIdentificacion?.DesconectarDriver(Dispositivo.Codigo);
            }
        }
    }
}
