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
    [Autorizacion(PermisosOrquestador.Pantalla)]
    public class PantallaController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public PantallaController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverPantalla>();
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
            Expression<Func<ConfigPantalla, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.UrlSCATO.Contains(filtro) || x.UrlFTP.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigPantalla, ConfigPantallaModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigPantallaModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigPantallaModel, ConfigPantalla>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var Pantalla = conversor.Convertir<ConfigPantalla, ConfigPantallaModel>(repositorio.Obtener<ConfigPantalla>(id));
            Pantalla.Dispositivo.ConcentradorId = Pantalla.Dispositivo.Concentrador != null ? Pantalla.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(Pantalla);
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
        public ActionResult Modificar(ConfigPantallaModel model)
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
                    viejo.ServerFijo = model.Dispositivo.ServerFijo;
                    var configPantalla = (ConfigPantalla) viejo.Configuracion;
                    configPantalla.UrlFTP = model.UrlFTP;
                    configPantalla.UrlSCATO = model.UrlSCATO;
                    configPantalla.TiempoDeRefresco = model.TiempoDeRefresco;
                    configPantalla.NombreUsuario = model.NombreUsuario;
                    configPantalla.Contrasenia = model.Contrasenia;
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
            var pantalla = repositorio.Obtener<ConfigPantalla>(id);
            
            repositorio.Remover(pantalla.Dispositivo);
            repositorio.Remover(pantalla);
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
            var camara = repositorio.Obtener<ConfigPantalla>(id);
            return View(camara);
        }

        public ActionResult TomarFoto(string codigo)
        {
            var resultado = servicio.Ejecutar(new EjecutarObtenerUltimaFoto
            {
                CodigoDispositivo = codigo
            });
            var resultado2 = (ResultadoTomarFoto)resultado;
            return File(resultado2.Imagen, "image/jpg");
        }

        public JsonResult ObtenerUltimaFoto(string codigo)
        {
            var resultado = (ResultadoTomarFoto)servicio.Ejecutar(new EjecutarObtenerUltimaFoto
            {
                CodigoDispositivo = codigo
            });
            return Json(new
            {
                imagen = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(resultado.Imagen))
            }, JsonRequestBehavior.AllowGet);
        }

    }
}