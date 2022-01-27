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
    [Autorizacion(PermisosOrquestador.Usuario)]
    public class UsuarioController : BaseController
    {
        private readonly IConversor conversor;

        public UsuarioController(IRepositorioFactory repositorio, IConversor conversor, IServicioOrquestador servicio, ILogger log)
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
            Expression<Func<Usuario, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Apellido.Contains(filtro) || x.Nombre.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            ViewBag.Items = repositorio.Listar(expresionFiltro, paginacion);
        }

        public ActionResult Crear()
        {
            SetearVistaConfiguracion();
            return View();
        }

        [HttpPost]
        public ActionResult Crear(Usuario model, string roles)
        {
            if (ModelState.IsValid && ValidacionesDeNegocio(model, roles))
            {
                var listaRoles = roles.FromJson<Rol[]>().Select(x => x.Id).ToList();
                model.RolesAsociados = repositorio.Listar<Rol>(x => listaRoles.Any(y => y == x.Id));
                repositorio.Agregar(model);
                repositorio.GuardarCambios();
                return new AjaxEditSuccessResult();
            }
            SetearVistaConfiguracion();
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var aModificar = repositorio.Obtener<Usuario>(id);
            SetearVistaConfiguracion();
            return View(aModificar);
        }

        [HttpPost]
        public ActionResult Modificar(Usuario model, string roles)
        {
            if (ModelState.IsValid && ValidacionesDeNegocio(model, roles))
            {
                var listaRoles = roles.FromJson<RolModel[]>().Select(x => x.Id).ToList();
                model = repositorio.Obtener<Usuario>(model.Id);
                if (model.RolesAsociados != null)
                {
                    model.RolesAsociados.Clear();
                }
                else
                {
                    model.RolesAsociados = new List<Rol>();
                }
                foreach (var rol in repositorio.Listar<Rol>(x => listaRoles.Any(y => y == x.Id)))
                {
                    model.RolesAsociados.Add(rol);
                }
                repositorio.GuardarCambios();
                return new AjaxEditSuccessResult();
            }
            
            SetearVistaConfiguracion();
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var usuario = repositorio.Obtener<Usuario>(id);

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

        public JsonResult ObtenerRoles(int id)
        {
            var usuario = repositorio.Obtener<Usuario>(id);
            var roles = usuario != null ? conversor.ConvertirList<Rol,RolModel>(usuario.RolesAsociados.ToList()) : null;
            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        private void SetearVistaConfiguracion()
        {
            var roles = repositorio.Listar<Rol>().OrderBy(p => p.Descripcion).Select(d => new SelectListItem { Text = d.Descripcion, Value = d.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            ViewBag.Roles = roles;
        }

        private bool ValidacionesDeNegocio(Usuario usuario, string roles)
        {
            if (repositorio.Existe<Usuario>(e => e.NombreUsuario == usuario.NombreUsuario && (e.Id != usuario.Id)))
            {
                ModelState.AddModelError("NombreUsuario", Textos.Usuario_NombreUsuarioExistente);
                return false;
            }
            if (String.IsNullOrEmpty(roles))
            {
                ModelState.AddModelError("RolesAsociados", string.Format(Textos.Error_Requerido, "Rol"));
                return false;
            }
            return true;
        }
    }
}
