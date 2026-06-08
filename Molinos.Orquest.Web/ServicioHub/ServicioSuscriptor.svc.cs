using System;
using System.Collections.Generic;
using Microsoft.AspNet.SignalR;
using Molinos.Orquest.Dominio.Dtos;
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
        private readonly HubClient hubClient;
        private readonly IServicioOrquestador servicioOrquestador;

        public ServicioSuscriptor(IRepositorioFactory factoryRepo, ILogger log, HubClientFactory hubClientFactory, IServicioOrquestador servicioOrquestador)
        {
            this.factoryRepo = factoryRepo;
            this.log = log;
            this.hubClient = hubClientFactory.GetClient();
            this.servicioOrquestador = servicioOrquestador;
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
                    else if (notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                    {
                        var patente = notificacion.Datos.ContainsKey("Patente") ? notificacion.Datos["Patente"] : null;
                        var hayError = string.IsNullOrEmpty(patente);
                        var mensaje = hayError && notificacion.Datos.ContainsKey("Error") 
                            ? notificacion.Datos["Error"] 
                            : patente ?? string.Empty;

                        hubClient.Invoke("NotificarLecturaVehiculo", new LecturaVehiculo
                        {
                            CodigoItc = codigoItc,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Patente = mensaje,
                            HayError = hayError
                        });
                    }
                    else if (notificacion.CodigoEvento == CodigosEventos.IdentificacionVehicular)
                    {
                        var configCIV = repositorio.Obtener<ConfigIdentificacionVehicular>(
                            c => c.Activo && c.Codigo == notificacion.CodigoDispositivo);

                        if (configCIV == null)
                        {
                            configCIV = repositorio.Obtener<ConfigIdentificacionVehicular>(
                                c => c.Activo && (
                                    (c.ConfigLectorTarjetas != null && c.ConfigLectorTarjetas.Dispositivo.Codigo == notificacion.CodigoDispositivo) ||
                                    (c.ConfigSensorVehicular != null && c.ConfigSensorVehicular.Dispositivo.Codigo == notificacion.CodigoDispositivo)));
                        }

                        if (configCIV == null)
                        {
                            log.Warn("IdentificacionVehicular: no se encontro ConfigIdentificacionVehicular activa para CodigoDispositivo='{0}'. Evento descartado.", notificacion.CodigoDispositivo);
                            return;
                        }

                        var detalles = notificacion.Datos.ContainsKey("Detalle")
                            ? JsonConvert.DeserializeObject<List<DetalleCamaraCIV>>(notificacion.Datos["Detalle"])
                            : new List<DetalleCamaraCIV>();

                        string valor          = notificacion.Datos.ContainsKey("Tarjeta")         ? notificacion.Datos["Tarjeta"]         : string.Empty;
                        string patenteCIV     = notificacion.Datos.ContainsKey("Patente")         ? notificacion.Datos["Patente"]         : null;
                        string fechaEvento    = notificacion.Datos.ContainsKey("FechaEvento")     ? notificacion.Datos["FechaEvento"]     : string.Empty;
                        bool vehiculoPresente = notificacion.Datos.ContainsKey("VehiculoPresente")
                            && bool.TryParse(notificacion.Datos["VehiculoPresente"], out var vp) && vp;

                        var notificacionCIV = new NotificacionCIV
                        {
                            CodigoCIV         = configCIV.Codigo,
                            CodigoEvento      = notificacion.CodigoEvento,
                            CodigoDispositivo = notificacion.CodigoDispositivo,
                            Valor             = valor,
                            VehiculoPresente  = vehiculoPresente,
                            Patente           = patenteCIV,
                            FechaEvento       = fechaEvento,
                            Detalles          = detalles,
                            JsonCompleto      = JsonConvert.SerializeObject(notificacion, Formatting.Indented)
                        };

                        var hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificaLectura>();
                        hubContext.Clients.Group("civ-" + notificacionCIV.CodigoCIV).actualizarEventoCIV(notificacionCIV);
                        log.Debug("NotificarEventoCIV despachado al grupo 'civ-{0}'.", notificacionCIV.CodigoCIV);
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
