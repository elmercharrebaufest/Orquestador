using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Models;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected readonly IRepositorio repositorio;
        protected readonly IServicioOrquestador servicio;
        protected readonly ILogger log;

        protected BaseController()
        {
            
        }

        protected BaseController(IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log)
        {
            this.repositorio = repositorio.Repositorio();
            this.servicio = servicio;
            this.log = log;
        }
        
        /// <summary>
        /// Manage the internationalization before to invokes the action in the current controller context.
        /// </summary>
        protected override void ExecuteCore()
        {
            var culture = CultureInfo.CurrentCulture;
            var cookie = new CookieUsuario();
            if (cookie.Valor("CurrentCulture") != "")
            {
                culture = CultureInfo.GetCultureInfo(int.Parse(cookie.Valor("CurrentCulture")));
            }
            SessionManager.CurrentCulture = culture;
            //
            // Invokes the action in the current controller context.
            //
            base.ExecuteCore();
        }

        protected override bool DisableAsyncSupport
        {
            get { return true; }
        }

        protected void SetearVistaConfiguracion(IEnumerable<string> drivers, bool puedeSerConcentrador = false)
        {
            ViewBag.PuedeSerConcentrador = puedeSerConcentrador;
            ViewBag.Drivers = drivers.Select(d => new SelectListItem { Text = d, Value = d }).ToList();
            var concentradores = repositorio.Listar<Dispositivo>(w => w.EsConcentrador).Select(d => new SelectListItem { Text = d.Descripcion, Value = d.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            concentradores.Insert(0, new SelectListItem { Selected = true, Text = Textos.NoTiene, Value = "0" });
            ViewBag.Concentradores = concentradores;
        }

        protected virtual bool ValidacionesDeNegocio(Dispositivo dispositivo)
        {
            if (repositorio.Existe<Dispositivo>(e => e.Codigo == dispositivo.Codigo && (dispositivo.Id == 0 || e.Id != dispositivo.Id)))
            {
                ModelState.AddModelError("Dispositivo.Codigo", Textos.Cabezal_CodigoExistente);
                return false;
            }
            if (repositorio.Existe<Dispositivo>(e => e.Id == dispositivo.Id && e.Codigo != dispositivo.Codigo))
            {
                if (repositorio.Existe<Dispositivo>(e => e.Id == dispositivo.Id && e.TomadoPor != null))
                {
                    ModelState.AddModelError("Dispositivo.Codigo", Textos.ConfigDispositivo_ErrorCambioCodigoTomado);
                    return false;
                }
                if (repositorio.Existe<Suscripcion>(s => s.Dispositivo.Id == dispositivo.Id))
                {
                    ModelState.AddModelError("Dispositivo.Codigo", Textos.ConfigDispositivo_ErrorCambioCodigo);
                    return false;
                }
            }
            if (dispositivo.EsConcentrador && dispositivo.ConcentradorId > 0)
            {
                ModelState.AddModelError("Dispositivo.ConcentradorId", Textos.ConcentradorError);
                return false;
            }
            return true;
        }

        protected void RecargarConfiguracion(string codigoDispositivo)
        {
            try
            {
                servicio.RecargarConfiguracion(codigoDispositivo);
            }
            catch (Exception e)
            {
                log.Error(e, "No se pudo recargar la configuración del dispositivo {0}", codigoDispositivo);
            }
        }

    }
}
