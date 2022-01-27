using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
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
    [Autorizacion(PermisosOrquestador.Camara, PermisosOrquestador.PruebaCamara)]
    public class CamaraController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public CamaraController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverCamara>();
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
            Expression<Func<ConfigCamara, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = repositorio.Listar(expresionFiltro, paginacion);
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.Camara)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Camara)]
        public ActionResult Crear(ConfigCamara model)
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
                    model.Contrasenia = SetearPassword(model.Contrasenia, null);
                    repositorio.Agregar(model);
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Camara)]
        public ActionResult Modificar(int id)
        {
            var camara = repositorio.Obtener<ConfigCamara>(id);
            camara.Dispositivo.ConcentradorId = camara.Dispositivo.Concentrador != null ? camara.Dispositivo.Concentrador.Id : 0;
            if (!string.IsNullOrEmpty(camara.Contrasenia)) camara.Contrasenia = Encriptador.Decrypt(camara.Contrasenia);

            SetearVistaConfiguracion(drivers);

            return View(camara);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Camara)]
        public ActionResult Modificar(ConfigCamara model)
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
                    var configCamara = (ConfigCamara)viejo.Configuracion;
                    configCamara.Uri = model.Uri;
                    configCamara.TimeoutLectura = model.TimeoutLectura;
                    configCamara.MargenDerecho = model.MargenDerecho;
                    configCamara.MargenInferior = model.MargenInferior;
                    configCamara.MargenIzquierdo = model.MargenIzquierdo;
                    configCamara.MargenSuperior = model.MargenSuperior;
                    configCamara.RotateFlipType = model.RotateFlipType;
                    configCamara.Contrasenia = SetearPassword(model.Contrasenia, null);
                    configCamara.NombreUsuario = model.NombreUsuario;
                    configCamara.UrlStreaming = model.UrlStreaming;

                    configCamara.DireccionIp = model.DireccionIp;
                    configCamara.Puerto = model.Puerto;
                    configCamara.LongFrase = model.LongFrase;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Camara)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var camara = repositorio.Obtener<ConfigCamara>(id);
            if (camara.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarCamaraPorOrquestado);
            }
            repositorio.Remover(camara.Dispositivo);
            repositorio.Remover(camara);
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

        [Autorizacion(PermisosOrquestador.PruebaCamara)]
        public ActionResult Probar(int id)
        {
            var camara = repositorio.Obtener<ConfigCamara>(id);
            return View(camara);
        }

        public ActionResult TomarFoto(string codigo)
        {
            var resultado = (ResultadoTomarFoto)servicio.Ejecutar(new EjecutarTomarFoto
            {
                CodigoDispositivo = codigo
            });
            return File(resultado.Imagen, "image/jpg");
        }

        public JsonResult ObtenerFotoConPatente(string codigo)
        {
            var resultado = (ResultadoObtenerPatente)servicio.Ejecutar(new EjecutarTomarFoto
            {
                CodigoDispositivo = codigo
            });
            return Json(new
            {
                imagen = String.Format("data:image/jpg;base64,{0}", Convert.ToBase64String(resultado.Imagen)),
                patente = resultado.Patente,
                confianza = resultado.Confianza
            }, JsonRequestBehavior.AllowGet);
        }

        public string SetearPassword(string newPassword, string currentPassword)
        {

            if (!string.IsNullOrEmpty(newPassword))
            {

                currentPassword = Encriptador.Encrypt(newPassword);
            }

            return currentPassword;
        }
    }
}