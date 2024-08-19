using Molinos.Orquest.Dominio.Resultados;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ControlDeNotificaciones : IAdministradorSuscripciones
    {
        /// <summary>
        /// Maximo de notificaciones soportadas por minuto para un dispositivo
        /// </summary>
        private int maxNotificacions;

        private readonly IAdministradorSuscripciones administradorNotificaciones;
        private readonly ILogger log;
        private IDictionary<string, IControlDeNotificacionesDispositivo> controles = new Dictionary<string, IControlDeNotificacionesDispositivo>();

        public ControlDeNotificaciones(IAdministradorSuscripciones administradorNotificaciones, ILogger log, int maxNotificacions)
        {
            this.administradorNotificaciones = administradorNotificaciones;
            this.log = log;
            this.maxNotificacions = maxNotificacions;
        }

        public void CancelarSuscripcion(int idSuscripcion, int idDispositivo, bool cancelarTodas)
        {
            administradorNotificaciones.CancelarSuscripcion(idSuscripcion, idDispositivo, cancelarTodas);
        }

        public void CancelarSuscripcionPorRutaAcceso(string rutaAccesoSuscriptor, int idDispositivo, bool cancelarTodas)
        {
            administradorNotificaciones.CancelarSuscripcionPorRutaAcceso(rutaAccesoSuscriptor, idDispositivo, cancelarTodas);
        }

        public int CrearSuscripcion(int idDispositivo, string codigoEvento, string rutaAccesoSuscriptor, bool persistente)
        {
            return administradorNotificaciones.CrearSuscripcion(idDispositivo, codigoEvento, rutaAccesoSuscriptor, persistente);
        }

        public void DepurarSuscripciones(int idDispositivo, DateTime vencimiento, bool depurarPersistentes)
        {
            administradorNotificaciones.DepurarSuscripciones(idDispositivo, vencimiento, depurarPersistentes);
        }

        public bool ExistenSuscripcionesPara(int idDispositivo)
        {
            return administradorNotificaciones.ExistenSuscripcionesPara(idDispositivo);
        }

        public void Notificar(NotificacionEvento evento)
        {
            try
            {
                administradorNotificaciones.Notificar(evento);
            }
            catch(Exception e)
            {
                log.Error(e, "Ocurrio un error al notificar el dispositivo `{0}` evento `{1}`", evento.CodigoDispositivo, evento.CodigoEvento);
                throw e;
            }
            finally
            {
                try
                {
                    IControlDeNotificacionesDispositivo control;
                    // Intento obtener un contador ya existente para el dispositivo y sino lo creo. 
                    if (!controles.TryGetValue(evento.CodigoDispositivo, out control))
                    {
                        // TODO: implementar inyeccion de instancias. Maximo configurable para cada dispositivo.
                        control = new ControlDeNotificacionesDispositivo(maxNotificacions);
                        controles.Add(evento.CodigoDispositivo, control);
                    }

                    // Incremento el contador. 
                    var exedido = control.NewValue();
                    if (exedido)
                    {
                        log.Warn("El dispositivo `{0}` está generando un numero de notificaciones que superan los `{1}/min` - Actual `{2}` desde `{3}`", evento.CodigoDispositivo, maxNotificacions, control.TotalCounter, control.TotalInit);
                    }
                }
                catch(Exception e)
                {
                    log.Error(e, "Ocurrio un error al controlar el dispositivo `{0}` evento `{1}`", evento.CodigoDispositivo, evento.CodigoEvento);
                }
            }
        }
    }
} 