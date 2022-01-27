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
using System.Web;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda, PermisosOrquestador.PruebaPantallaPuestoDeVianda)]
    public class PantallaPuestoDeViandaController : BaseController
    {
        private readonly IEnumerable<string> drivers;
        private readonly IConversor conversor;


        public PantallaPuestoDeViandaController(IDriverFactory driverFactory, IConversor conversor, IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverPantallaPuestoDeVianda>();
            this.conversor = conversor;
        }

        public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            FillViewBag();
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



        [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda)]
        public ActionResult Crear()
        {
            FillViewBag();
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda)]
        public ActionResult Crear(ConfigPantallaPuestoDeViandaModel model)
        {
            FillViewBag();
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
                    var puesto = repositorio.Obtener<ConfigPuestoDeVianda>(model.ConfigPuestoDeViandas);
                    repositorio.Agregar(new ConfigPantallaPuestoDeVianda { 
                    Dispositivo= model.Dispositivo,
                    ConfigPuestoDeVianda=puesto,
                    ClaseDriver= model.ClaseDriver
                    });
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda)]
        public ActionResult Modificar(int id)
        {
            FillViewBag();
            var puestoDeVianda = repositorio.Obtener<ConfigPantallaPuestoDeVianda, ConfigPantallaPuestoDeViandaModel>(x=>x.Id== id, x=> 
            new ConfigPantallaPuestoDeViandaModel { 
            Dispositivo =x.Dispositivo,
            Id=x.Id,
            ClaseDriver=x.ClaseDriver,
            ConfigPuestoDeViandas=x.ConfigPuestoDeVianda.Id
            });           
            puestoDeVianda.Dispositivo.ConcentradorId = puestoDeVianda.Dispositivo.Concentrador != null ? puestoDeVianda.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(puestoDeVianda);
        }


        [HttpPost]
        [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda)]
        public ActionResult Modificar(ConfigPantallaPuestoDeViandaModel model)
        {
            FillViewBag();
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<ConfigPantallaPuestoDeVianda>(model.Id);

                    viejo.Dispositivo.Codigo = model.Dispositivo.Codigo;
                    viejo.Dispositivo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.ClaseDriver = model.ClaseDriver;
                    viejo.Dispositivo.Activo = model.Dispositivo.Activo;
                    viejo.Dispositivo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    viejo.ConfigPuestoDeVianda = repositorio.Obtener<ConfigPuestoDeVianda>(model.ConfigPuestoDeViandas);
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }
        [HttpPost]
        [Autorizacion(PermisosOrquestador.PantallaPuestoDeVianda)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var puestoDeVianda = repositorio.Obtener<ConfigPantallaPuestoDeVianda>(id);
            if (puestoDeVianda.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarPantallaPuestoDeViandaPorOrquestado);
            }
            repositorio.Remover(puestoDeVianda.Dispositivo);
            repositorio.Remover(puestoDeVianda);
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
            var puestoDeVianda = conversor.Convertir<ConfigPantallaPuestoDeVianda, ListaPantallaPuestoDeVianda>(repositorio.Obtener<ConfigPantallaPuestoDeVianda>(id));
            return View(puestoDeVianda);
        }


        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigPantallaPuestoDeVianda, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro)  || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigPantallaPuestoDeVianda, ListaPantallaPuestoDeVianda>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }
        private void FillViewBag()
        {
            var listaDePantallas = repositorio.Listar<ConfigPuestoDeVianda>().Select(
                    x => new SelectListItem
                    {
                        Text = x.Dispositivo.Descripcion,
                        Value = x.Id.ToString(),
                        Selected = false
                    }).OrderBy(x => x.Value);
            ViewBag.Puestos = listaDePantallas;

            
        }
    }
}