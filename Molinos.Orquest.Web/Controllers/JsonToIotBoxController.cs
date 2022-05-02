using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
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

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Barrera)]
    public class JsonToIotBoxController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public JsonToIotBoxController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log): base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverJsonToIotBox>();
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
            Expression<Func<ConfigJsonToIotBox, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro)  || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }

        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            SetearVistaConfiguracion();
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigJsonToIotBox model)
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
            SetearVistaConfiguracion();
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var barrera = repositorio.Obtener<ConfigJsonToIotBox>(id);
            barrera.Dispositivo.ConcentradorId = barrera.Dispositivo.Concentrador != null ? barrera.Dispositivo.Concentrador.Id : 0;
            //barrera.SensorId = barrera.Sensor != null ? barrera.Sensor.Id : 0;
            SetearVistaConfiguracion(drivers);
            SetearVistaConfiguracion();
            return View(barrera);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigJsonToIotBox model)
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
                    var configBarrera = (ConfigJsonToIotBox) viejo.Configuracion;
                    configBarrera.NumeroSalida = model.NumeroSalida;
                    configBarrera.Json_Id = model.Json_Id;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            SetearVistaConfiguracion();
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var barrera = repositorio.Obtener<ConfigJsonToIotBox>(id);
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

        private bool ValidacionesDeNegocio(ConfigJsonToIotBox config, Dispositivo dispositivo)
        {
            
            if (repositorio.Existe<ConfigJsonToIotBox>(x => x.Id != config.Id && x.NumeroSalida == config.NumeroSalida && x.Dispositivo.Concentrador.Id == dispositivo.ConcentradorId))
            {
                ModelState.AddModelError("NumeroSalida", string.Format(Textos.Barrera_SalidaEnUso, config.NumeroSalida));
                
            }
           
            if (!ModelState.IsValid)
            {
                return false;
            }
            return ValidacionesDeNegocio(dispositivo);
        }

        private void SetearVistaConfiguracion()
        {
            var sensores = repositorio.Listar<FormatosJson>().OrderBy(p => p.Dispositivo_nombre).Select(d => new SelectListItem { Text = d.Dispositivo_nombre, Value = d.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            ViewBag.Sensores = sensores;
        }
    }
}