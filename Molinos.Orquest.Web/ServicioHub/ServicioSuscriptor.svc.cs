using System;
using System.Configuration;
using Microsoft.AspNet.SignalR.Client;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.ServicioHub
{
    public class ServicioSuscriptor : IServicioSuscriptor
    {
        private readonly IRepositorioFactory factoryRepo;
        private readonly ILogger log;
        private static IHubProxy hubProxy;
        private readonly HubClient hubClient;

        private static readonly object LockObject = new object();

        public ServicioSuscriptor(IRepositorioFactory factoryRepo, ILogger log, HubClientFactory hubClientFactory)
        {
            this.factoryRepo = factoryRepo;
            this.log = log;
            this.hubClient = hubClientFactory.GetClient();
        }

        public void Recibir(NotificacionEvento notificacion)
        {
            try
            {
       
                log.Info("Evento recibido Recibir :", JsonConvert.SerializeObject(notificacion, Formatting.Indented));
                log.Debug("Evento recibido: {0}", notificacion);
                using (var repositorio = factoryRepo.Repositorio())
                {
                    log.Info("1");
                    var codigoItc = repositorio.Obtener<Dispositivo, string>(x => x.Codigo == notificacion.CodigoDispositivo, x => x.Concentrador.Codigo);

                    if (notificacion.CodigoEvento == CodigosEventos.LecturaTarjetaRecibida)
                    {
                        log.Info("2");
                        log.Debug("Informando lectura de tarjeta...");
                        hubClient.Invoke("NotificarLecturaTarjeta", new LecturaTarjeta
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Valor = notificacion.Datos["Tarjeta"],
                        });
                    }
                    else if (notificacion.CodigoEvento == CodigosEventos.EntradaActivada
                                || notificacion.CodigoEvento == CodigosEventos.EntradaDesactivada)
                    {
                        log.Info("3");
                        log.Debug("Informando lectura de entrada...");
                        hubClient.Invoke("NotificarLecturaEntrada", new LecturaEntrada
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Valor = notificacion.CodigoEvento == CodigosEventos.EntradaActivada,
                        });
                    }
                    else if (notificacion.CodigoEvento == CodigosEventos.ErrorConexionDispositivo
                        || notificacion.CodigoEvento == CodigosEventos.ConexionDispositivoCorrecta)
                    {
                        log.Info("4");
                        log.Debug("Informando estado dispositivo...");
                        hubClient.Invoke("NotificarEstadoDispositivo", new EstadoDispositivo
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Error = notificacion.CodigoEvento == CodigosEventos.ErrorConexionDispositivo,
                            Mensaje = notificacion.CodigoEvento == CodigosEventos.ErrorConexionDispositivo ? notificacion.Datos["Error"] : null,
                        });
                    }
                    //TODO: Deprecar
                    else if (notificacion.CodigoEvento == CodigosEventos.LecturaTarjetaMolinete)
                    {
                        log.Info("5");
                        log.Debug("Informando estado dispositivo...");
                        hubClient.Invoke("NotificarLecturaTarjetaMolinete", new LecturaTarjetaMolinete
                        {
                            CodigoMolinete = notificacion.CodigoDispositivo,
                            Tarjeta = notificacion.Datos["Tarjeta"],
                            Sentido = notificacion.Datos["Sentido"]
                        });
                    }
                    //TODO: Deprecar
                    else if (notificacion.CodigoEvento == CodigosEventos.NuevoTransito)
                    {
                        log.Info("6");
                        log.Debug("Informando estado dispositivo...");
                        hubClient.Invoke("NotificarTransitoMolinete", new TransitoMolinete
                        {
                            CodigoMolinete = notificacion.CodigoDispositivo,
                            Tarjeta = notificacion.Datos["Tarjeta"],
                            Direccion = notificacion.Datos["Direccion"],
                            Denegado = bool.Parse(notificacion.Datos["Denegado"]),
                            SinTransito = bool.Parse(notificacion.Datos["SinTransito"])
                        });
                    }
                    else if (notificacion.CodigoEvento == CodigosEventos.LecturaQr)
                    {
                        log.Info("7");
                        log.Debug("Informando estado dispositivo...");
                        hubClient.Invoke("NotificarLecturaDni", new LecturaQr
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            CodigoMolinete = notificacion.CodigoDispositivo,
                            QR = notificacion.Datos["QR"],
                            Lector = notificacion.Datos["Lector"]
                        });
                    }
                    else if (notificacion.CodigoEvento == CodigosEventos.CambioEstadoIntercomunicador)
                    {
                        log.Info("8");
                        log.Info("Informando estado dispositivo... CambioEstadoIntercomunicador");
                        var estados = notificacion.Datos["Dato"].Split(';');
                        log.Info("Informando estado dispositivo... Mic" + estados[0].ToLower());
                        log.Info("Informando estado dispositivo... Speaker" + estados[1].ToLower());
                        hubClient.Invoke("NotificarCambioEstadoIntercomunicador", new EstadoIntercomunicador
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Mic = estados[0].ToLower() == "true",
                            Speaker = estados[1].ToLower() == "true",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo informar el evento.");
            }
        }
    }
}
