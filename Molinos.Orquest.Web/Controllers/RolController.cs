using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Rol)]
    public class RolController : BaseController
    {
        private readonly IConversor conversor;

        public RolController(IRepositorioFactory repositorio, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
        }

        public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View();
        }

        [AjaxOnly]
        [ActionName("Index")]
        public ActionResult Listar(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View("Listar");
        }


        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<Rol, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Descripcion.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            ViewBag.Items = repositorio.Listar(expresionFiltro, paginacion);
        }

        public ActionResult Crear()
        {
            ViewBag.Permisos = Enum.GetValues(typeof(PermisosOrquestador)).Cast<PermisosOrquestador>().Select(d => new SelectListItem { Text = d.DisplayEnum(), Value = ((int)d).ToString(CultureInfo.InvariantCulture) }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Crear(RolModel model, string permisos)
        {
            if (ModelState.IsValid && ValidacionesDeNegocio(model,permisos))
            {

                var listaPermisos = permisos.FromJson<PermisoModel[]>().Select(x => (PermisosOrquestador)x.Id).ToList();
                var entidad = conversor.Convertir<RolModel, Rol>(model);
                entidad.PermisosAsociados = listaPermisos.Select(x =>new RolPermisoOrquestador{ PermisoOrquestador = x,Rol = entidad}).ToList();
                repositorio.Agregar(entidad);
                repositorio.GuardarCambios();
                return new AjaxEditSuccessResult();
            }
            ViewBag.Permisos = Enum.GetValues(typeof(PermisosOrquestador)).Cast<PermisosOrquestador>().Select(d => new SelectListItem { Text = d.DisplayEnum(), Value = ((int)d).ToString(CultureInfo.InvariantCulture) }).ToList();
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var aModificar = conversor.Convertir<Rol, RolModel>(repositorio.Obtener<Rol>(id));
            ViewBag.Permisos = Enum.GetValues(typeof(PermisosOrquestador)).Cast<PermisosOrquestador>().Select(d => new SelectListItem { Text = d.DisplayEnum(), Value = ((int)d).ToString(CultureInfo.InvariantCulture) }).ToList();

            return View(aModificar);
        }

        [HttpPost]
        public ActionResult Modificar(RolModel model, string permisos)
        {
            if (ModelState.IsValid && ValidacionesDeNegocio(model, permisos))
            {
                var listaPermisos = permisos.FromJson<PermisoModel[]>().ToList();

                var entidad = repositorio.Obtener<Rol>(model.Id);
                entidad.Descripcion = model.Descripcion;
                if (entidad.PermisosAsociados != null)
                {
                    foreach (var permiso in entidad.PermisosAsociados.ToList())
                    {
                        repositorio.Remover(permiso);
                    }
                }
                else
                {
                    entidad.PermisosAsociados = new List<RolPermisoOrquestador>();
                }
                foreach (var permiso in listaPermisos)
                {

                    entidad.PermisosAsociados.Add(new RolPermisoOrquestador { PermisoOrquestador = (PermisosOrquestador)permiso.Id, Rol = entidad});
                }
                repositorio.GuardarCambios();
                return new AjaxEditSuccessResult();
            }
            ViewBag.Permisos = Enum.GetValues(typeof(PermisosOrquestador)).Cast<PermisosOrquestador>().Select(d => new SelectListItem { Text = d.DisplayEnum(), Value = ((int)d).ToString(CultureInfo.InvariantCulture) }).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var usuario = repositorio.Obtener<Rol>(id);

            repositorio.Remover(usuario);
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

        public JsonResult ObtenerPermisos(int id)
        {
            var rol = repositorio.Obtener<Rol>(id);
            var permisos = rol != null ? rol.PermisosAsociados.Select(x => x.PermisoOrquestador).ToList() : new List<PermisosOrquestador>();

            var jsonPermisos = new List<Object>();
            foreach (var permiso in permisos)
            {
                jsonPermisos.Add(new
                        {
                            Id = permiso,
                            Descripcion = permiso.DisplayEnum()
                        });
            }

            return Json(jsonPermisos, JsonRequestBehavior.AllowGet);
        }

        private bool ValidacionesDeNegocio(RolModel rol, string permisos)
        {
            if (repositorio.Existe<Rol>(e => e.Descripcion == rol.Descripcion && (e.Id != rol.Id)))
            {
                ModelState.AddModelError("Descripcion", Textos.Rol_Existente);
                return false;
            }
            if (String.IsNullOrEmpty(permisos) || permisos == "[]")
            {
                ModelState.AddModelError("Permisos", Textos.Rol_RolSinPermisos);
                return false;
            }
            return true;
        }
    }
}
