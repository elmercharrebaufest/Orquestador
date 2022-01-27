using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Models;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Itc, PermisosOrquestador.PruebaItc)]
    public class PruebaItcController : BaseController
    {
        public PruebaItcController(IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
        }

        public ActionResult Index(int id)
        {
            try
            {
                log.Debug("Conectando a ITC Id: {0}", id);
                log.Debug("Obteniendo dispositivos...");
                var dispositivo = repositorio.Obtener<Dispositivo>(id);
                var barreras = repositorio.Listar<ConfigBarrera, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.NumeroSalida.ToString() }).ToList();
                var sensores = repositorio.Listar<ConfigSensor, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.NumeroEntrada.ToString() }).ToList();
                var lectores = repositorio.Listar<ConfigLectorTarjetas, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.Lector }).ToList();
                var lectoresQr = repositorio.Listar<ConfigLectorQr, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.Lector }).ToList();
                var displays = repositorio.Listar<ConfigDisplay, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.NumeroSalida.ToString() }).ToList();
                var cortinaAgua = repositorio.Listar<ConfigCortinaAgua, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.NumeroSalida.ToString() }).ToList();
                var tags = repositorio.Listar<ConfigTag, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.Query.ToString() }).ToList();
                var comunicadores = repositorio.Listar<ConfigComunicador, PruebaDispositivoModel>(x => x.Dispositivo.Concentrador.Id == id, x => new PruebaDispositivoModel { Codigo = x.Dispositivo.Codigo, Numero = x.NumeroSalida.ToString() }).ToList();

                var errores = new List<string>();
                var configItc = (ConfigItc) dispositivo.Configuracion;
                var model = new PruebaItcModel
                {
                    CodigoItc = dispositivo.Codigo,
                    DireccionIpItc = configItc.DireccionIp,
                    PuertoItc = configItc.Puerto,
                    Lectores = lectores,
                    Barreras = barreras,
                    LectoresQr = lectoresQr,
                    Sensores = sensores,
                    Displays = displays,
                    CortinaAgua = cortinaAgua,
                    Tags = tags,
                    Comunicadores = comunicadores
                };
                log.Debug("Suscribiendo eventos de lectores");
                var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];
                foreach (var lector in lectores)
                {
                    Suscribir(lector.Codigo, CodigosEventos.LecturaTarjetaRecibida, urlSuscriptor, errores);
                    Suscribir(lector.Codigo, CodigosEventos.ErrorConexionDispositivo, urlSuscriptor, errores);
                    Suscribir(lector.Codigo, CodigosEventos.ConexionDispositivoCorrecta, urlSuscriptor, errores);
                }
                foreach (var lector in lectoresQr)
                {
                    Suscribir(lector.Codigo, CodigosEventos.LecturaQr, urlSuscriptor, errores);
                    Suscribir(lector.Codigo, CodigosEventos.ErrorConexionDispositivo, urlSuscriptor, errores);
                    Suscribir(lector.Codigo, CodigosEventos.ConexionDispositivoCorrecta, urlSuscriptor, errores);
                }
                log.Debug("Suscribiendo eventos de entradas");
                foreach (var sensor in sensores)
                {
                    Suscribir(sensor.Codigo, CodigosEventos.EntradaActivada, urlSuscriptor, errores);
                    Suscribir(sensor.Codigo, CodigosEventos.EntradaDesactivada, urlSuscriptor, errores);
                    Suscribir(sensor.Codigo, CodigosEventos.ErrorConexionDispositivo, urlSuscriptor, errores);
                    Suscribir(sensor.Codigo, CodigosEventos.ConexionDispositivoCorrecta, urlSuscriptor, errores);
                }
                foreach (var cortinaAg in cortinaAgua)
                {
                    Suscribir(cortinaAg.Codigo, CodigosEventos.EntradaActivada, urlSuscriptor, errores);
                    Suscribir(cortinaAg.Codigo, CodigosEventos.ErrorConexionDispositivo, urlSuscriptor, errores);
                    Suscribir(cortinaAg.Codigo, CodigosEventos.ConexionDispositivoCorrecta, urlSuscriptor, errores);
                }

                var intercomunicadorDispositivoList = new List<IntercomunicadorDispositivoDto>();
                foreach (var comunicador in comunicadores.OrderBy(q=>q.Numero))
                {
                    var intercomunicadorConfig = GetIntercomunicadorDispositivoConfig(comunicador.Codigo);
                    intercomunicadorConfig.UniqueId = comunicador.Numero.ToString();
                    intercomunicadorDispositivoList.Add(intercomunicadorConfig);
                }
                ViewBag.InterComunicadorDispositivoList = intercomunicadorDispositivoList;
                ViewBag.Errores = errores;
                ViewBag.IdItc = id;
                ViewBag.ServerIp = GetIPAddress();

                log.Debug("Proceso de conexión completo.");
                return View(model);

            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo conectar al dispositivo ITC Id: {0}", id);
                throw;
            }
        }

        private void Suscribir(string codigoDisp, string codigoEvento, string urlSuscriptor, List<string> errores)
        {
            try
            {
                var resultado = servicio.Suscribir(new ComandoSuscribir
                {
                    CodigoDispositivo = codigoDisp,
                    CodigoEvento = codigoEvento,
                    RutaAccesoSuscriptor = urlSuscriptor
                });

                if (resultado.Mensaje.Codigo != 0)
                {
                    var mensaje = String.Format("{0}: {1}-{2}", codigoDisp, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
                    log.Warn(mensaje);
                    errores.Add(mensaje);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                errores.Add(String.Format("{0}: {1}", codigoDisp, Textos.PruebaItc_ErrorServicio));
            }
        }

        public ActionResult ActivarSalida(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                log.Info($"Activar Salida codigo: {codigo}");
                var resultado = servicio.Ejecutar(new EjecutarAperturaBarrera { CodigoDispositivo = codigo });
                jsonResult = Json(new
                    {
                        Codigo = resultado.Mensaje.Codigo,
                        Mensaje = String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion)
                    }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult =  Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
        public ActionResult DesactivarSalida(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                log.Info($"Desactivar Salida codigo: {codigo}");
                var resultado = servicio.Ejecutar(new EjecutarCierreBarrera { CodigoDispositivo = codigo });
                jsonResult = Json(new
                {
                    Codigo = resultado.Mensaje.Codigo,
                    Mensaje = String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion)
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult = Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
        public ActionResult ActivarDisplay(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarActivarDisplay { CodigoDispositivo = codigo });
                jsonResult = Json(new
                {
                    Codigo = resultado.Mensaje.Codigo,
                    Mensaje = String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion)
                }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult = Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult Desuscribir(string codigoItc)
        {
            try
            {
                var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];
                var suscripciones = repositorio.Listar<Suscripcion>(x => x.RutaAccesoSuscriptor == urlSuscriptor && x.Dispositivo.Concentrador.Codigo == codigoItc);
                foreach (var suscripcion in suscripciones)
                {
                    var resultado = servicio.CancelarSuscripcion(new ComandoCancelarSuscripcion
                        {
                            IdSuscripcion = suscripcion.Id,
                            CodigoDispositivo = suscripcion.Dispositivo.Codigo,
                            CancelarTodas = true
                        });
                    log.Debug("Cancelando Suscripcion {0}: {1}-{2}", suscripcion.Dispositivo.Codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
                }
            }
            catch (Exception e)
            {
                log.Error(e, "No se pudieron cancelar todas las suscripciones");
            }
            return Json(string.Empty, JsonRequestBehavior.AllowGet);
        }

        [Autorizacion(PermisosOrquestador.PruebaItc, PermisosOrquestador.Itc)]
        public JsonResult ConsultarEstado(string codigoDispositivo)
        {
            try
            {
                log.Debug("Consultando estado ITC {0}", codigoDispositivo);
                var respuesta =
                    servicio.Ejecutar(new EjecutarVerificacionDispositivo { CodigoDispositivo = codigoDispositivo });
                return Json(new
                {
                    Conectado = respuesta.Mensaje.Codigo == Codigos.OK,
                    Estado = respuesta.Mensaje.Codigo == Codigos.OK ? Textos.Conectado : Textos.Desconectado,
                    Mensaje = respuesta.Mensaje.Descripcion
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                log.Error(e, "Ocurrió un error al consultar el estado del ITC {0}", codigoDispositivo);
                return Json(new
                {
                    Conectado = false,
                    Estado = Textos.Desconectado,
                    Mensaje = Textos.PruebaItc_ErrorServicio
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EjecutarTag(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarEjecutarQuery { CodigoDispositivo = codigo });

                jsonResult = Json(new
                {
                    Codigo = resultado.Mensaje.Codigo,
                    Mensaje = string.Join("<br />", ((ResultadoEjecutarQuery)resultado).queryResult.ToArray()) // String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult = Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public string GetIPAddress()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName()); 
            IPAddress ipAddress = ipHostInfo.AddressList.Where(x=>x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault();

            return ipAddress.ToString();
        }

        [HttpPost]
        public ActionResult PrenderApagarDispositivo(string codigoDispositivo, bool activar)
        {
            var urlServer = new Uri(ConfigurationManager.AppSettings["ICWebServerUrl"]);
            var resultado = servicio.Ejecutar(new EjecutarComunicador { CodigoDispositivo = codigoDispositivo, Activar = activar, Tipo = Dominio.Enums.TipoComunicador.Both, ServerComunicador = urlServer.Host });
            return Json(resultado);
        }

        private IntercomunicadorDispositivoDto GetIntercomunicadorDispositivoConfig(string codigoComunicador)
        {

            var intercomunicadorDispositivo = new IntercomunicadorDispositivoDto
            {
                Codigo = codigoComunicador,
                ICPCConfig = ConfigurationManager.AppSettings["ICPCConfig"],
                ICWebServerUrl = ConfigurationManager.AppSettings["ICWebServerUrl"],
                ICWSServerUrl = ConfigurationManager.AppSettings["ICWSServerUrl"],
                DeviceActivationUrl = Url.Action("PrenderApagarDispositivo", "PruebaItc"),
                PublishingPathListen = codigoComunicador + Constantes.IntercomunicadorDireccion.HaciaLaWeb,
                PublishingPathSpeak = Constantes.IntercomunicadorDireccion.DesdeLaWeb + codigoComunicador,
            };

            return intercomunicadorDispositivo;
        }
    }
}