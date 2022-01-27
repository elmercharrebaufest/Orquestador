using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
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
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.ImpresoraHasar, PermisosOrquestador.PruebaImpresoraHasar)]
    public class ImpresoraHasarController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public ImpresoraHasarController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverImpresoraHasar>();
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
            Expression<Func<ConfigImpresoraHasar, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigImpresoraHasar, ConfigImpresoraHasarModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.ImpresoraHasar)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.ImpresoraHasar)]
        public ActionResult Crear(ConfigImpresoraHasarModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    model.Dispositivo.Codigo = model.Dispositivo.Codigo.ToUpper();
                    repositorio.Agregar(conversor.Convertir<ConfigImpresoraHasarModel, ConfigImpresoraHasar>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.ImpresoraHasar)]
        public ActionResult Modificar(int id)
        {
            var nirs = conversor.Convertir<ConfigImpresoraHasar, ConfigImpresoraHasarModel>(repositorio.Obtener<ConfigImpresoraHasar>(id));
            nirs.Dispositivo.ConcentradorId = nirs.Dispositivo.Concentrador != null ? nirs.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(nirs);
        }

        private string CaracterValido(string delimitadorCampos)
        {
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(delimitadorCampos.ToCharArray()); 
            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            if (asciiChars.Length > 0){
                if (asciiChars[0] >= 0 && asciiChars[0] <= 31)
                {
                    return Server.UrlEncode(delimitadorCampos);
                }
            }
            return delimitadorCampos;
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.ImpresoraHasar)]
        public ActionResult Modificar(ConfigImpresoraHasarModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<Dispositivo>(model.Id);
                    viejo.Codigo = model.Dispositivo.Codigo.ToUpper();
                    viejo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.Configuracion.ClaseDriver = model.ClaseDriver;
                    viejo.Activo = model.Dispositivo.Activo;
                    viejo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    var configimp = (ConfigImpresoraHasar) viejo.Configuracion;
                    configimp.DireccionIp = model.DireccionIp;
                    configimp.Puerto = model.Puerto;
                    configimp.TimeoutLectura = model.TimeoutLectura;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.ImpresoraHasar)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var impresora = repositorio.Obtener<ConfigImpresoraHasar>(id);
            if (impresora.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarNirsPorOrquestador);
            }
            repositorio.Remover(impresora.Dispositivo);
            repositorio.Remover(impresora);
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
            var imp = conversor.Convertir<ConfigImpresoraHasar, ConfigImpresoraHasarModel>(repositorio.Obtener<ConfigImpresoraHasar>(id));
            return View(imp);
        }

        public ActionResult ObtenerAnalisis(string codigo, string material = "SOJA")
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarAnalisis { CodigoDispositivo = codigo, Material = material});
                if(resultado.Mensaje.Codigo == Codigos.OK)
                {
                    foreach (var r in resultado.Valores)
                    {
                        resultados.Add(new ResultadoPruebaModel(r.Key + " - " + r.Value, true));
                    }
                }
                else
                {
                    resultados.Add(new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error al obtener el análisis del dispositivo {0}", codigo);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }
            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        public JsonResult ConsultarEstado(string codigoDispositivo)
        {
            try
            {
                log.Debug("Consultando estado NIRS {0}", codigoDispositivo);
                var respuesta = servicio.Ejecutar(new EjecutarVerificacionDispositivo { CodigoDispositivo = codigoDispositivo });
                return Json(new
                {
                    Conectado = respuesta.Mensaje.Codigo == Codigos.OK,
                    Estado = respuesta.Mensaje.Codigo == Codigos.OK ? Textos.Conectado : Textos.Desconectado,
                    Mensaje = respuesta.Mensaje.Descripcion
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                log.Error(e, "Ocurrió un error al consultar el estado del ITC {0}", codigoDispositivo);
                return Json(new
                {
                    Conectado = false,
                    Estado = Textos.Desconectado,
                    Mensaje = Textos.PruebaItc_ErrorServicio
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}