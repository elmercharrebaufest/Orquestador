using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Suscripcion)]
    public class SuscripcionController : BaseController
    {
        public SuscripcionController(IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
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
            Expression<Func<Suscripcion, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.RutaAccesoSuscriptor.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var suscripcion = repositorio.Obtener<Suscripcion>(id);
            var contenido = "true";
            try
            {
                var resultado = servicio.CancelarSuscripcion(new ComandoCancelarSuscripcion
                {
                    IdSuscripcion = suscripcion.Id,
                    CodigoDispositivo = suscripcion.Dispositivo.Codigo,
                    CancelarTodas = true
                });
                log.Debug("Cancelando Suscripcion {0}: {1}-{2}", suscripcion.Dispositivo.Codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);

                if (resultado.Mensaje.Codigo != Codigos.OK && Request.IsAjaxRequest())
                {
                    contenido = resultado.Mensaje.Descripcion;
                }
            }
            catch (EntidadReferenciadaException e)
            {
                log.Error(e, "No se pudieron cancelar todas las suscripciones");
                if (Request.IsAjaxRequest())
                {
                    contenido = Textos.Error_EliminarReferenciado;
                }
            }
            catch(Exception e)
            {
                contenido = e.Message;
            }

            if (Request.IsAjaxRequest())
            {
                return Content(contenido);
            }
            return RedirectToAction("Index");
        }
        
    }
}