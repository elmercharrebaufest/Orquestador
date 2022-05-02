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
    [Autorizacion(PermisosOrquestador.CortinaAgua)]
    public class CortinaDeAguaController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public CortinaDeAguaController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log): base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverCortinaAgua>();
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
            Expression<Func<ConfigCortinaAgua, bool>> expresionFiltro = null;
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
        public ActionResult Crear(ConfigCortinaAgua model)
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
            var cortinaAgua = repositorio.Obtener<ConfigCortinaAgua>(id);
            cortinaAgua.Dispositivo.ConcentradorId = cortinaAgua.Dispositivo.Concentrador != null ? cortinaAgua.Dispositivo.Concentrador.Id : 0;
            cortinaAgua.EstacionId = cortinaAgua.Estacion != null ? cortinaAgua.Estacion.Id : 0;
            SetearVistaConfiguracion(drivers);
            SetearVistaConfiguracion();
            return View(cortinaAgua);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigCortinaAgua model)
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
                    viejo.ServerFijo = model.Dispositivo.ServerFijo;
                    var configCortinaAgua = (ConfigCortinaAgua)viejo.Configuracion;
                    configCortinaAgua.NumeroSalida = model.NumeroSalida;
                    configCortinaAgua.IntervaloPooling = model.IntervaloPooling;
                    configCortinaAgua.DireccionDelVientoDesde = model.DireccionDelVientoDesde;
                    configCortinaAgua.DireccionDelVientoHasta = model.DireccionDelVientoHasta; 
                    configCortinaAgua.EstadoAbierta = model.EstadoAbierta;
                    configCortinaAgua.TiempoActivacion = model.TiempoActivacion;
                    configCortinaAgua.TiempoDeEsperaActivacion = model.TiempoDeEsperaActivacion;
                    configCortinaAgua.Estacion = repositorio.Obtener<ConfigMeteorologica>(model.EstacionId);
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
            var cortinaAgua = repositorio.Obtener<ConfigCortinaAgua>(id);
            if (cortinaAgua.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarCortinaAguaPorOrquestado);
            }
            repositorio.Remover(cortinaAgua.Dispositivo);
            repositorio.Remover(cortinaAgua);
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

        private bool ValidacionesDeNegocio(ConfigCortinaAgua config, Dispositivo dispositivo)
        {

            if (repositorio.Existe<ConfigCortinaAgua>(x => x.Id != config.Id && x.NumeroSalida == config.NumeroSalida && x.Dispositivo.Concentrador.Id == dispositivo.ConcentradorId))
            {
                ModelState.AddModelError("NumeroSalida", string.Format(Textos.Barrera_SalidaEnUso, config.NumeroSalida));

            }
            if (config.TiempoDeEsperaActivacion == null || config.TiempoDeEsperaActivacion.Value <= 0)
            {
                ModelState.AddModelError("TiempoEsperaActivacion", string.Format(Textos.Error_Requerido, Textos.CortinaAgua_TiempoEsperaActivacion));

            }
            if (config.TiempoActivacion == null || config.TiempoActivacion.Value <= 0)
            {
                ModelState.AddModelError("TiempoMaximoEsperaReintento", string.Format(Textos.Error_Requerido, Textos.CortinaAgua_TiempoActivacion));

            }
            if (config.DireccionDelVientoDesde <= 0)
            {
                ModelState.AddModelError("DireccionDelVientoDesde", string.Format(Textos.Error_Requerido, Textos.CortinaAgua_DireccionVientoDesde));

            }
            if (config.DireccionDelVientoHasta <= 0)
            {
                ModelState.AddModelError("DierccionDelVientoHasta", string.Format(Textos.Error_Requerido, Textos.CortinaAgua_DireccionVientoHasta));

            }
            if (config.DireccionDelVientoDesde > config.DireccionDelVientoHasta)
            {
                ModelState.AddModelError("DireccionDelVientoDesde", string.Format(Textos.Error_DireccionVientoDesde, Textos.CortinaAgua_DireccionVientoDesde));
            }
            if (config.DireccionDelVientoHasta < config.DireccionDelVientoDesde)
            {
                ModelState.AddModelError("DireccionDelVientoHasta", string.Format(Textos.Error_DireccionVientoHasta, Textos.CortinaAgua_DireccionVientoHasta));
            }
            if(config.DireccionDelVientoDesde <0 && config.DireccionDelVientoDesde> 360)
            {
                ModelState.AddModelError("DireccionDelVientoDesde", string.Format(Textos.Error_RangoNumerico, 0, 360));
            }
            if (config.DireccionDelVientoHasta < 0 && config.DireccionDelVientoHasta > 360)
            {
                ModelState.AddModelError("DireccionDelVientoHasta", string.Format(Textos.Error_RangoNumerico, 0, 360));
            }
            if (!ModelState.IsValid)
            {
                return false;
            }
            return ValidacionesDeNegocio(dispositivo);
        }

        private void SetearVistaConfiguracion()
        {
            var estacion = repositorio.Listar<ConfigMeteorologica>().OrderBy(p => p.Dispositivo.Descripcion).Select(d => new SelectListItem { Text = d.Dispositivo.Descripcion, Value = d.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            ViewBag.Estacion = estacion;
        }
    }
}