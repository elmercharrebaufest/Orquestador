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
    [Autorizacion(PermisosOrquestador.LectorQr, PermisosOrquestador.PruebaLectorQr)]
    public class LectorQrController : BaseController
    {
        private readonly IEnumerable<string> drivers;
        private readonly IConversor conversor;


        public LectorQrController(IDriverFactory driverFactory, IConversor conversor, IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverLectorQr>();
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



        [Autorizacion(PermisosOrquestador.LectorQr)]
        public ActionResult Crear()
        {
            
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.LectorQr)]
        public ActionResult Crear(ConfigLectorQr model)
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

                    repositorio.Agregar(model);
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();

                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.LectorQr)]
        public ActionResult Modificar(int id)
        {
            
            var LectorQr = repositorio.Obtener<ConfigLectorQr>(id);
            LectorQr.Dispositivo.ConcentradorId = LectorQr.Dispositivo.Concentrador != null ? LectorQr.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(LectorQr);
        }


        [HttpPost]
        [Autorizacion(PermisosOrquestador.LectorQr)]
        public ActionResult Modificar(ConfigLectorQr model)
        {
            
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<ConfigLectorQr>(model.Id);

                    viejo.Dispositivo.Codigo = model.Dispositivo.Codigo;
                    viejo.Dispositivo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.ClaseDriver = model.ClaseDriver;
                    viejo.Dispositivo.Activo = model.Dispositivo.Activo;
                    viejo.Dispositivo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    viejo.Dispositivo.ServerFijo = model.Dispositivo.ServerFijo;

                    viejo.Lector = model.Lector;

                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }
        [HttpPost]
        [Autorizacion(PermisosOrquestador.LectorQr)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var LectorQr = repositorio.Obtener<ConfigLectorQr>(id);
            if (LectorQr.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarLectorQrPorOrquestado);
            }
            repositorio.Remover(LectorQr.Dispositivo);
            repositorio.Remover(LectorQr);
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
        
        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<ConfigLectorQr, bool>> expresionFiltro = null;
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