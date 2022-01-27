using Molinos.Orquest.Servicios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ninject.Extensions.Logging;
using Molinos.Orquest.Dominio.Entidades;
using System.Configuration;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Web.Models;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Dominio.Seguridad;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Molinete, PermisosOrquestador.PruebaMolinete)]
    public class PruebaMolineteController : BaseController
    {
        public PruebaMolineteController(IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
        }

        public ActionResult Index(int id)
        {
            try
            {
                log.Debug("Conectando a Molinete Id: {0}", id);
                log.Debug("Obteniendo dispositivos...");
                var dispositivo = repositorio.Obtener<Dispositivo>(id);

                var errores = new List<string>();
                var configMolinete = (ConfigMolinete)dispositivo.Configuracion;
                var model = new PruebaMolineteModel
                {
                    CodigoMolinete = dispositivo.Codigo,
                    DireccionUrl = configMolinete.DireccionUrl
                };
                log.Debug("Suscribiendo eventos de lectores");
                var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];

                log.Debug("Suscribiendo eventos de entradas");

                Suscribir(configMolinete, CodigosEventos.LecturaTarjetaMolinete, urlSuscriptor, errores);
                Suscribir(configMolinete, CodigosEventos.NuevoTransito, urlSuscriptor, errores);

                ViewBag.Errores = errores;
                ViewBag.IdMolinete = id;
                log.Debug("Proceso de conexión completo.");
                return View(model);
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo conectar al dispositivo Molinete Id: {0}", id);
                throw;
            }
        }

        public ActionResult HabilitarTransito(string codigo, string Direccion)
        {
            JsonResult jsonResult;
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarHabilitarTransito { CodigoDispositivo = codigo, Direccion = Direccion });
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

        private void Suscribir(ConfigDispositivo configDisp, string codigoEvento, string urlSuscriptor, List<string> errores)
        {
            var codigoDisp = configDisp.Dispositivo.Codigo;
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
        public ActionResult Desuscribir(string codigoMolinete)
        {
            try
            {
                var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];
                var suscripciones = repositorio.Listar<Suscripcion>(x => x.RutaAccesoSuscriptor == urlSuscriptor && x.Dispositivo.Concentrador.Codigo == codigoMolinete);
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
    }
}