# Referencia: Controller

## Estructura base

```csharp
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
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.{NombrePermiso})]
    public class {NombreDominio}Controller : BaseController
    {
        private readonly IConversor conversor;

        public {NombreDominio}Controller(
            IRepositorioFactory repositorio, IConversor conversor,
            IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
        }

        // GET /Index  (full page)
        public ActionResult Index(string filtro, int pagina = 1,
            string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View((object)filtro);
        }

        // GET /Index  (ajax refresh)
        [AjaxOnly]
        [ActionName("Index")]
        public ActionResult Listar(string filtro, int pagina = 1,
            string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            return View("Listar", (object)filtro);
        }

        private void ListQuery(string filtro, int pagina, string ordenarPor, DirOrden dirOrden)
        {
            Expression<Func<{EntidadDominio}, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Nombre.Contains(filtro) || x.Codigo.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            ViewBag.Items = repositorio.Listar(expresionFiltro, paginacion);
        }

        public ActionResult Crear()
        {
            PopularDropdowns();
            return View();
        }

        [HttpPost]
        public ActionResult Crear({NombreDominio}Model model)
        {
            ValidarModelo(model);
            if (ModelState.IsValid)
            {
                var entidad = new {EntidadDominio}
                {
                    // mapear campos
                };
                repositorio.Agregar(entidad);
                repositorio.GuardarCambios();
                return RedirectToAction("Index");
            }
            PopularDropdowns();
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var entidad = repositorio.Obtener<{EntidadDominio}>(id);
            var model = conversor.Convertir<{EntidadDominio}, {NombreDominio}Model>(entidad);
            PopularDropdowns();
            return View(model);
        }

        [HttpPost]
        public ActionResult Modificar({NombreDominio}Model model)
        {
            ValidarModelo(model);
            if (ModelState.IsValid)
            {
                var entidad = repositorio.Obtener<{EntidadDominio}>(model.Id);
                // asignar campos
                repositorio.GuardarCambios();
                return RedirectToAction("Index");
            }
            PopularDropdowns();
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var entidad = repositorio.Obtener<{EntidadDominio}>(id);
            repositorio.Remover(entidad);
            try
            {
                repositorio.GuardarCambios();
            }
            catch (EntidadReferenciadaException)
            {
                if (Request.IsAjaxRequest())
                    return Content(Textos.Error_EliminarReferenciado);
            }
            if (Request.IsAjaxRequest())
                return Content("true");

            return RedirectToAction("Index");
        }

        private void PopularDropdowns()
        {
            // Ejemplo con SelectListItem:
            // var items = repositorio.Listar<OtraEntidad>()
            //     .Select(x => new SelectListItem {
            //         Text = x.Dispositivo.Descripcion + " (" + x.Dispositivo.Codigo + ")",
            //         Value = x.Id.ToString(CultureInfo.InvariantCulture)
            //     }).ToList();
            // items.Insert(0, new SelectListItem { Text = "— Seleccione —", Value = "" });
            // ViewBag.Items = items;
        }

        private void ValidarModelo({NombreDominio}Model model)
        {
            if (repositorio.Existe<{EntidadDominio}>(
                e => e.Codigo == model.Codigo && (model.Id == 0 || e.Id != model.Id)))
            {
                ModelState.AddModelError("Codigo", Textos.Camara_CodigoExistente);
            }
        }
    }
}
```

## Colecciones de navegación EF (proxy)

Cuando la entidad tiene una colección hija (ej: `ICollection<EntidadHija> Hijos`), **nunca reasignar** en Modificar:

```csharp
// ❌ INCORRECTO — lanza InvalidOperationException en EF proxy
entidad.Hijos = BuildHijos(ids);

// ✅ CORRECTO
foreach (var hijo in entidad.Hijos.ToList())
    repositorio.Remover(hijo);

foreach (var hijo in BuildHijos(ids))
    entidad.Hijos.Add(hijo);
```

## Pasar colecciones como DTOs al ViewBag

Cuando se necesita serializar al cliente, usar la clase anidada del Model en lugar de anonymous types para que los nombres de propiedad sean estables:

```csharp
ViewBag.AvailableItems = repositorio.Listar<EntidadBase>()
    .Select(x => new {NombreDominio}Model.ItemAnidadoModel
    {
        Id     = x.Id,
        Nombre = x.Dispositivo.Descripcion + " (" + x.Dispositivo.Codigo + ")",
        Ip     = x.Uri ?? ""
    }).ToList();
```
