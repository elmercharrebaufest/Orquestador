using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.LectorTarjetas)]
    public class LectorTarjetasController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public LectorTarjetasController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverLectorTarjetas>();
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

        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigLectorTarjetas, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.Lector.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }

        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigLectorTarjetas model)
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
                if (ValidacionesDeNegocio(model, model.Dispositivo))
                {
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(model);
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var lectorTarjetas = repositorio.Obtener<ConfigLectorTarjetas>(id);
            lectorTarjetas.Dispositivo.ConcentradorId = lectorTarjetas.Dispositivo.Concentrador != null ? lectorTarjetas.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(lectorTarjetas);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigLectorTarjetas model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model, model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<Dispositivo>(model.Id);

                    viejo.Codigo = model.Dispositivo.Codigo;
                    viejo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.Configuracion.ClaseDriver = model.ClaseDriver;
                    viejo.Activo = model.Dispositivo.Activo;
                    viejo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    var configLectorTarjetas = (ConfigLectorTarjetas)viejo.Configuracion;
                    configLectorTarjetas.Lector = model.Lector;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var lectorTarjetas = repositorio.Obtener<ConfigLectorTarjetas>(id);
            if (lectorTarjetas.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarLectorDeTarjetasPorOrquestado);
            }
            repositorio.Remover(lectorTarjetas.Dispositivo);
            repositorio.Remover(lectorTarjetas);
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

        private bool ValidacionesDeNegocio(ConfigLectorTarjetas config, Dispositivo dispositivo)
        {
            //if (repositorio.Existe<ConfigLectorTarjetas>(x => x.Id != config.Id && x.Lector == config.Lector && x.Dispositivo.Concentrador.Id == dispositivo.ConcentradorId))
            //{
            //    ModelState.AddModelError("Lector", string.Format(Textos.LectorTarjetas_LectorEnUso, config.Lector));
            //    return false;
            //}
            return ValidacionesDeNegocio(dispositivo);
        }
    }
}