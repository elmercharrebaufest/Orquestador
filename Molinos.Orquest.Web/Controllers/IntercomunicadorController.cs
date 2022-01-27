using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Dtos;
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Historian)]
    public class IntercomunicadorController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public IntercomunicadorController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            try
            {
                drivers = driverFactory.DriversDisponibles<IDriverComunicador>();

                //The code that causes the error goes here.
            }
            catch (ReflectionTypeLoadException ex)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Exception exSub in ex.LoaderExceptions)
                {
                    sb.AppendLine(exSub.Message);
                    FileNotFoundException exFileNotFound = exSub as FileNotFoundException;
                    if (exFileNotFound != null)
                    {
                        if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                        {
                            sb.AppendLine("Fusion Log:");
                            sb.AppendLine(exFileNotFound.FusionLog);
                        }
                    }
                    sb.AppendLine();
                }
                string errorMessage = sb.ToString();
                log.Error(ex, errorMessage);
                //Display or log the error based on your application.
            }
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
            Expression<Func<ConfigComunicador, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigComunicador, ConfigComunicadorModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.Historian)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Historian)]
        public ActionResult Crear(ConfigComunicadorModel model)
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
                    repositorio.Agregar(conversor.Convertir<ConfigComunicadorModel, ConfigComunicador>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Historian)]
        public ActionResult Modificar(int id)
        {
            var comunicador = conversor.Convertir<ConfigComunicador, ConfigComunicadorModel>(repositorio.Obtener<ConfigComunicador>(id));
            comunicador.Dispositivo.ConcentradorId = comunicador.Dispositivo.Concentrador != null ? comunicador.Dispositivo.Concentrador.Id : 0;

            SetearVistaConfiguracion(drivers);
            return View(comunicador);
        }

        private string CaracterValido(string carInicioFrase)
        {
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(carInicioFrase.ToCharArray());
            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            if (asciiChars.Length > 0)
            {
                if (asciiChars[0] >= 0 && asciiChars[0] <= 31)
                {
                    return Server.UrlEncode(carInicioFrase);
                }
            }
            return carInicioFrase;
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Historian)]
        public ActionResult Modificar(ConfigComunicadorModel model)
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
                    var configComunicador = (ConfigComunicador)viejo.Configuracion;
                    configComunicador.NumeroSalida = model.NumeroSalida;

                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Historian)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var comunicador = repositorio.Obtener<ConfigComunicador>(id);
            if (comunicador.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarCabezalPorOrquestado);
            }
            repositorio.Remover(comunicador.Dispositivo);
            repositorio.Remover(comunicador);
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
            var comunicador = conversor.Convertir<ConfigComunicador, ConfigComunicadorModel>(repositorio.Obtener<ConfigComunicador>(id));
            var intercomunicadorConfig = GetIntercomunicadorDispositivoConfig(comunicador.Dispositivo.Codigo);
            intercomunicadorConfig.UniqueId = comunicador.Dispositivo.Codigo.ToString();
            ViewBag.InterComunicadorDispositivo = intercomunicadorConfig;
            return View(comunicador);
        }

        public ActionResult ObtenerPeso(string codigo)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarPesaje { CodigoDispositivo = codigo });
                resultados.Add(resultado.Mensaje.Codigo == Codigos.OK
                                   ? new ResultadoPruebaModel(resultado.Valores["Pesaje"].ToString(CultureInfo.CurrentUICulture), false)
                                   : new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
            }
            catch (Exception e)
            {
                log.Error(e, "Error al obtener el peso del cabezal {0}", codigo);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }

            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        [HttpPost]
        public ActionResult PrenderApagarDispositivo(string codigoDispositivo, bool activar)
        {
            var urlServer = new Uri(ConfigurationManager.AppSettings["ICWebServerUrl"]);
            var resultado = servicio.Ejecutar(new EjecutarComunicador { CodigoDispositivo = codigoDispositivo, Activar = activar, Tipo = Dominio.Enums.TipoComunicador.Both, ServerComunicador = urlServer.Host });
            return Json(resultado);
        }

        private IntercomunicadorDispositivoDto GetIntercomunicadorDispositivoConfig(string codigoComunicador)
        {
            var intercomunicadorDispositivo = new IntercomunicadorDispositivoDto
            {
                Codigo = codigoComunicador,
                ICPCConfig = ConfigurationManager.AppSettings["ICPCConfig"],
                ICWebServerUrl = ConfigurationManager.AppSettings["ICWebServerUrl"],
                ICWSServerUrl = ConfigurationManager.AppSettings["ICWSServerUrl"],
                DeviceActivationUrl = Url.Action("PrenderApagarDispositivo", "Intercomunicador"),
                PublishingPathListen = codigoComunicador + Constantes.IntercomunicadorDireccion.HaciaLaWeb,
                PublishingPathSpeak = Constantes.IntercomunicadorDireccion.DesdeLaWeb + codigoComunicador,
            };

            return intercomunicadorDispositivo;
        }
    }
}