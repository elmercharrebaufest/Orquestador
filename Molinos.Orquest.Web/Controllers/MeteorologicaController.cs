using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.AbmMeteorologica)]
    public class MeteorologicaController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public MeteorologicaController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverMeteorologica>();
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
            Expression<Func<ConfigMeteorologica, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }

        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigMeteorologica model)
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
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var meteorologica = repositorio.Obtener<ConfigMeteorologica>(id);
            meteorologica.Dispositivo.ConcentradorId = meteorologica.Dispositivo.Concentrador != null ? meteorologica.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(meteorologica);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigMeteorologica model)
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
                    viejo.Activo = model.Dispositivo.Activo;
                    var configMeteorologica = (ConfigMeteorologica)viejo.Configuracion;
                    configMeteorologica.Ruta = model.Ruta;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var meteorologica = repositorio.Obtener<ConfigMeteorologica>(id);
            if (meteorologica.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarMeteorologicaPorOrquestador);
            }
            repositorio.Remover(meteorologica.Dispositivo);
            repositorio.Remover(meteorologica);
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
            var meteorologica = repositorio.Obtener<ConfigMeteorologica>(id);
            try
            {

          
                var resultado = servicio.Ejecutar(new EjecutarEstacionMeteorologica
                {
                    CodigoDispositivo = meteorologica.Dispositivo.Codigo
                });
                if (resultado.Mensaje != null && resultado.Mensaje.Codigo != 0)
                {
                    ViewBag.Error = resultado.Mensaje.ToString();

                }
                else
                {
                    //List<byte[]> bytes = ((ResultadoMeteorologica)resultado).Imagenes;
                    //List<List<byte[]>> grupos = bytes
                    //    .Select((x, i) => new { Index = i, Value = x })
                    //    .GroupBy(x => x.Index / 4)
                    //    .Select(x => x.Select(v => v.Value).ToList())
                    //    .ToList();

                    ViewBag.Data = ((ResultadoMeteorologica)resultado).Imagenes;
                }
                return View(meteorologica);
            }
            catch(Exception msg)
            {
                ViewBag.Error = msg.Message;
            }
            return View(meteorologica);
        }
    }
}
