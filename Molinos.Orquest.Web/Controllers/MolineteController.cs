using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Molinete, PermisosOrquestador.PruebaMolinete)]
    public class MolineteController : BaseController
    {
        private readonly IEnumerable<string> drivers;
        private readonly IConversor conversor;


        public MolineteController(IDriverFactory driverFactory, IConversor conversor, IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverMolinete>();
            this.conversor = conversor;
        }

        public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View((object)filtro);
        }

        [AjaxOnly]
        [ActionName("Index")]
        public ActionResult Listar(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View("Listar", (object)filtro);
        }



        [Autorizacion(PermisosOrquestador.Molinete)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Molinete)]
        public ActionResult Crear(ConfigMolineteModel model)
        {
            if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
            {
                model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
            }
            else
            {
                model.Dispositivo.Concentrador = null;
            }
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigMolineteModel, ConfigMolinete>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Molinete)]
        public ActionResult Modificar(int id)
        {
            var molinete = conversor.Convertir<ConfigMolinete, ConfigMolineteModel>(repositorio.Obtener<ConfigMolinete>(id));
            molinete.Dispositivo.ConcentradorId = molinete.Dispositivo.Concentrador != null ? molinete.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(molinete);
        }


        [HttpPost]
        [Autorizacion(PermisosOrquestador.Molinete)]
        public ActionResult Modificar(ConfigMolineteModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<Dispositivo>(model.Id);

                    viejo.Codigo = model.Dispositivo.Codigo;
                    viejo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.Configuracion.ClaseDriver = model.ClaseDriver;
                    viejo.Activo = model.Dispositivo.Activo;
                    viejo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    viejo.ServerFijo = model.Dispositivo.ServerFijo;

                    var configMolinete = (ConfigMolinete)viejo.Configuracion;

                    configMolinete.DireccionUrl = model.DireccionUrl;
                    configMolinete.IntervaloPooling = model.IntervaloPooling;
                    configMolinete.TimeoutHabilitacion = model.TimeoutHabilitacion;

                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }
        [HttpPost]
        [Autorizacion(PermisosOrquestador.Molinete)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var molinete = repositorio.Obtener<ConfigMolinete>(id);
            if (molinete.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarMolinetePorOrquestado);
            }
            repositorio.Remover(molinete.Dispositivo);
            repositorio.Remover(molinete);
            try
            {
                repositorio.GuardarCambios();
            }
            catch (EntidadReferenciadaException)
            {
                if (Request.IsAjaxRequest())
                {
                    contenido = Textos.Error_EliminarReferenciado;
                }
            }
            if (Request.IsAjaxRequest())
            {
                return Content(contenido);
            }
            return RedirectToAction("Index");
        }
        public ActionResult Probar(int id)
        {
            return RedirectToAction("Index", "PruebaMolinete", new { id });
        }

        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigMolinete, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionUrl.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigMolinete, ConfigMolineteModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }
    }
}