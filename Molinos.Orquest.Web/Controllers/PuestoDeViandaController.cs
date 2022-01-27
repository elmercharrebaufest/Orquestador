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
    [Autorizacion(PermisosOrquestador.PuestoDeVianda, PermisosOrquestador.PruebaPuestoDeVianda)]
    public class PuestoDeViandaController : BaseController
    {
        private readonly IEnumerable<string> drivers;
        private readonly IConversor conversor;


        public PuestoDeViandaController(IDriverFactory driverFactory, IConversor conversor, IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverPuestoDeVianda>();
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



        [Autorizacion(PermisosOrquestador.PuestoDeVianda)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.PuestoDeVianda)]
        public ActionResult Crear(ConfigPuestoDeViandaModel model)
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
                    repositorio.Agregar(conversor.Convertir<ConfigPuestoDeViandaModel, ConfigPuestoDeVianda>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.PuestoDeVianda)]
        public ActionResult Modificar(int id)
        {

            var puestoVianda = conversor.Convertir<ConfigPuestoDeVianda, ConfigPuestoDeViandaModel>(repositorio.Obtener<ConfigPuestoDeVianda>(id));
            puestoVianda.Dispositivo.ConcentradorId = puestoVianda.Dispositivo.Concentrador != null ? puestoVianda.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(puestoVianda);
        }


        [HttpPost]
        [Autorizacion(PermisosOrquestador.PuestoDeVianda)]
        public ActionResult Modificar(ConfigPuestoDeViandaModel model)
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

                    var configPuestoVianda = (ConfigPuestoDeVianda)viejo.Configuracion;
                    
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }
        [HttpPost]
        [Autorizacion(PermisosOrquestador.PuestoDeVianda)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var puestoVianda = repositorio.Obtener<ConfigPuestoDeVianda>(id);
            if (puestoVianda.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarPuestoDeViandaPorOrquestado);
            }
            repositorio.Remover(puestoVianda.Dispositivo);
            repositorio.Remover(puestoVianda);
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
            var puestoVianda = conversor.Convertir<ConfigPuestoDeVianda, ConfigPuestoDeViandaModel>(repositorio.Obtener<ConfigPuestoDeVianda>(id));
            return View(puestoVianda);
        }


        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigPuestoDeVianda, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) ||  x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigPuestoDeVianda, ConfigPuestoDeViandaModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }
    }
}