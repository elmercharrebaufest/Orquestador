using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Impl
{
    public class AdministradorSuscripciones : IAdministradorSuscripciones
    {
        private readonly IRepositorioFactory repositorioFactory;
        private readonly IServicioRemotoFactory servicioRemotoFactory;
        private readonly ILogger log;

        public AdministradorSuscripciones(IRepositorioFactory repositorioFactory,  IServicioRemotoFactory servicioRemotoFactory, ILogger log)
        {
            this.repositorioFactory = repositorioFactory;
            this.log = log;
            this.servicioRemotoFactory = servicioRemotoFactory;
        }

        public int CrearSuscripcion(int idDispositivo, string codigoEvento, string rutaAccesoSuscriptor, bool persistente)
        {
            log.Debug("Creando suscripción. Dispositivo: {0} Evento: {1} Ruta Suscriptor: {2}", 
                idDispositivo, codigoEvento, rutaAccesoSuscriptor);

            using (var repositorio = repositorioFactory.Repositorio())
            {
                var suscripcion = repositorio.Obtener<Suscripcion>(
                    s => s.Dispositivo.Id == idDispositivo
                         && s.CodigoEvento == codigoEvento
                         && s.RutaAccesoSuscriptor == rutaAccesoSuscriptor);
                
                if (suscripcion == null)
                {
                    suscripcion = new Suscripcion
                        {
                            Dispositivo = repositorio.Obtener<Dispositivo>(idDispositivo),
                            CodigoEvento = codigoEvento,
                            RutaAccesoSuscriptor = rutaAccesoSuscriptor,
                            Cantidad = 1,
                            UltimaSuscripcion = DateTime.Now,
                            Persistente = persistente
                        };
                    repositorio.Agregar(suscripcion);
                    log.Debug("Suscripción Id={3} creada. Dispositivo: {0} Evento: {1} Ruta Suscriptor: {2}",
                        idDispositivo, codigoEvento, rutaAccesoSuscriptor, suscripcion.Id);
                }
                else
                {
                    suscripcion.Cantidad += 1;
                    suscripcion.UltimaSuscripcion = DateTime.Now;
                    suscripcion.Persistente = persistente || suscripcion.Persistente;
                    log.Debug("Se incrementó el contador de siscriptiones de Id={3} a {4}. Dispositivo: {0} Evento: {1} Ruta Suscriptor: {2}",
                        idDispositivo, codigoEvento, rutaAccesoSuscriptor, suscripcion.Id, suscripcion.Cantidad);
                }
                repositorio.GuardarCambios();
                return suscripcion.Id;
            }
        }

        public void CancelarSuscripcion(int idSuscripcion, int idDispositivo, bool cancelarTodas)
        {
            log.Debug("Cancelando suscripción. Dispositivo: {0} Id Suscripción: {1}", idDispositivo,idSuscripcion);
            using (var repositorio = repositorioFactory.Repositorio())
            {
                var suscripcion = repositorio.Obtener<Suscripcion>(s => s.Id == idSuscripcion && s.Dispositivo.Id == idDispositivo);
                if (suscripcion == null)
                {
                    throw new SuscripcionNoEncontradaException(string.Format("No se encontró la suscripción {0} para el dispositivo {1}", idSuscripcion, idDispositivo));
                }

                if (suscripcion.Cantidad == 1 || cancelarTodas)
                {
                    repositorio.Remover(suscripcion);
                }
                else
                {
                    suscripcion.Cantidad -= 1;
                }
                repositorio.GuardarCambios();
            }
        }

        public void CancelarSuscripcionPorRutaAcceso(string rutaAccesoSuscriptor, int idDispositivo, bool cancelarTodas)
        {
            log.Debug("Cancelando suscripción. Dispositivo: {0} ruta de acceso: {1}", idDispositivo, rutaAccesoSuscriptor);
            using (var repositorio = repositorioFactory.Repositorio())
            {
                var suscripciones = repositorio.Listar<Suscripcion>(s => s.RutaAccesoSuscriptor == rutaAccesoSuscriptor && s.Dispositivo.Id == idDispositivo);
                if (!suscripciones.Any())
                {
                    throw new SuscripcionNoEncontradaException(string.Format("No se encontró la suscripción con ruta de acceso{0} para el dispositivo {1}", rutaAccesoSuscriptor, idDispositivo));
                }
                foreach(var suscripcion in suscripciones)
                {
                    if (suscripcion.Cantidad == 1 || cancelarTodas)
                    {
                        repositorio.Remover(suscripcion);
                    }
                    else
                    {
                        suscripcion.Cantidad -= 1;
                    }
                }
                
                repositorio.GuardarCambios();
            }
        }


        public void Notificar(NotificacionEvento evento)
        {
            log.Debug("Notificando Evento => {0}", evento);
            IList<string> suscriptores = new List<string>();
            using (var repositorio = repositorioFactory.Repositorio())
            {   
                suscriptores = repositorio.Listar<Suscripcion, string>(
                    s => s.Dispositivo.Codigo == evento.CodigoDispositivo && s.CodigoEvento == evento.CodigoEvento,
                    s => s.RutaAccesoSuscriptor);
                log.Debug("Se encontraron {0} suscripciones para el evento {1}-{2}", suscriptores.Count, evento.CodigoDispositivo, evento.CodigoEvento);
            }
            Task.Run(() => { 
                    var notificados = suscriptores.AsParallel().Sum(
                        rutaSuscriptor =>
                        {
                            var notificado = 0;
                            try
                            {
                                log.Debug("Notificando {0}-{1} a suscriptor: {2}",
                                    evento.CodigoDispositivo, evento.CodigoEvento, rutaSuscriptor);
                                using (var suscriptor = servicioRemotoFactory.CrearServicioSuscriptor(rutaSuscriptor))
                                {
                                    suscriptor.Servicio.Recibir(evento);
                                }
                                notificado = 1;
                            }
                            catch (Exception e)
                            {
                                log.Warn(e, "Falló la notificación de {0}-{1} a suscriptor: {2}",
                                                           evento.CodigoDispositivo, 
                                                           evento.CodigoEvento,
                                                           rutaSuscriptor);
                            }
                            return notificado;
                        });
                    log.Debug("Se notificaron {0} de {1} suscripciones al evento {2}-{3}", notificados,  suscriptores.Count, evento.CodigoDispositivo, evento.CodigoEvento);
                });
            log.Debug("Fin notificar suscripciones para el evento {0}-{1}", evento.CodigoDispositivo, evento.CodigoEvento);
        }

        public bool ExistenSuscripcionesPara(int idDispositivo)
        {
            using (var repositorio = repositorioFactory.Repositorio())
            {
                return repositorio.Existe<Suscripcion>(s => s.Dispositivo.Id == idDispositivo);
            }
        }

        public void DepurarSuscripciones(int idDispositivo, DateTime vencimiento, bool depurarPersistentes)
        {
            using (var repositorio = repositorioFactory.Repositorio())
            {
                var suscripcionesVencidas = 
                    repositorio.Listar<Suscripcion>(s => s.Dispositivo.Id == idDispositivo && s.UltimaSuscripcion < vencimiento && (depurarPersistentes || !s.Persistente));
                foreach (var suscripcion in suscripcionesVencidas)
                {
                    repositorio.Remover(suscripcion);
                }
                repositorio.GuardarCambios();
            }
        }
    }
}
