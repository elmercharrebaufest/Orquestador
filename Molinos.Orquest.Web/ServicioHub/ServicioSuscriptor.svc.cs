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
                        var estados = notificacion.Datos["Dato"].Split(';');
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
