using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.GrupoBarrera)]
    public class GrupoBarreraController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public GrupoBarreraController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverAgrupadorBarrera>();
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

        public ActionResult Crear()
        {
            SetearVistaConfiguracion();
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigGrupoBarreraModel model)
        {
            if (ModelState.IsValid)
            {
                var configGrupoBarrera = new ConfigGrupoBarrera
                {
                    Id = -1,
                    BarreraArribaId = model.BarreraArribaId,
                    BarreraAbajoId = model.BarreraAbajoId,
                    SensorArribaId = model.SensorArribaId,
                    SensorAbajoId = model.SensorAbajoId,
                    SensorPrimerCruceId = model.SensorPrimerCruceId,
                    SensorSegundoCruceId = model.SensorSegundoCruceId,
                    ClaseDriver = model.ClaseDriver
                };
                var dispositivo = new Dispositivo
                {
                    Id = configGrupoBarrera.Id,
                    Codigo = model.Codigo,
                    Activo = model.Activo,
                    Descripcion = model.Descripcion,
                };

                configGrupoBarrera.Dispositivo = dispositivo;

                repositorio.Agregar(configGrupoBarrera);
                repositorio.GuardarCambios();

                return new AjaxEditSuccessResult();
            }

            SetearVistaConfiguracion();
            return View();
        }

        //[Autorizacion(PermisosOrquestador.AgrupadorBarrera)]
        public ActionResult Modificar(int id)
        {
            var grupoBarrera = repositorio.Obtener<ConfigGrupoBarrera>(id);
            var grupoBarreraModel = conversor.Convertir<ConfigGrupoBarrera, ConfigGrupoBarreraModel>(repositorio.Obtener<ConfigGrupoBarrera>(id));
            grupoBarreraModel.Codigo = grupoBarrera.Dispositivo.Codigo;
            grupoBarreraModel.Descripcion = grupoBarrera.Dispositivo.Descripcion;
            grupoBarreraModel.Activo = grupoBarrera.Dispositivo.Activo;
            SetearVistaConfiguracion();
            return View(grupoBarreraModel);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigGrupoBarreraModel model)
        {
            if (ModelState.IsValid)
            {
                ConfigGrupoBarrera configGrupoBarrera = repositorio.Obtener<ConfigGrupoBarrera>(model.Id);

                configGrupoBarrera.Dispositivo.Codigo = model.Codigo;
                configGrupoBarrera.Dispositivo.Descripcion = model.Descripcion;
                configGrupoBarrera.Dispositivo.Activo = model.Activo;
                configGrupoBarrera.BarreraArribaId = model.BarreraArribaId;
                configGrupoBarrera.BarreraAbajoId = model.BarreraAbajoId;
                configGrupoBarrera.SensorArribaId = model.SensorArribaId;
                configGrupoBarrera.SensorAbajoId = model.SensorAbajoId;
                configGrupoBarrera.SensorPrimerCruceId = model.SensorPrimerCruceId;
                configGrupoBarrera.SensorSegundoCruceId = model.SensorSegundoCruceId;

                repositorio.GuardarCambios();
                RecargarConfiguracion(model.Codigo);
                return new AjaxEditSuccessResult();
            }

            SetearVistaConfiguracion();
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var barrera = repositorio.Obtener<ConfigGrupoBarrera>(id);
            if (barrera.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarBarreraPorOrquestado);
            }
            repositorio.Remover(barrera.Dispositivo);
            repositorio.Remover(barrera);
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
            return RedirectToAction("Index", "PruebaItc", new { id });
        }

        private void SetearVistaConfiguracion()
        {
            var sensores = repositorio.Listar<ConfigSensor>().OrderBy(p => p.Dispositivo.Descripcion).Select(d => new SelectListItem { Text = d.Dispositivo.Descripcion, Value = d.Id.ToString() }).ToList();
            var barreras = repositorio.Listar<ConfigBarrera>().OrderBy(p => p.Dispositivo.Descripcion).Select(d => new SelectListItem { Text = d.Dispositivo.Descripcion, Value = d.Id.ToString() }).ToList();

            ViewBag.Drivers = drivers.Select(d => new SelectListItem { Text = d, Value = d }).ToList();
            ViewBag.Sensores = sensores;
            ViewBag.Barreras = barreras;
        }

        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigGrupoBarrera, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }
    }
}