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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Comunicador)]
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

        [Autorizacion(PermisosOrquestador.Comunicador)]
        public ActionResult Crear()
        {
            ViewBag.Sensores = new List<SelectListItem>();
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [Autorizacion(PermisosOrquestador.Comunicador)]
        public ActionResult Modificar(int id)
        {
            var comunicador = conversor.Convertir<ConfigComunicador, ConfigComunicadorModel>(repositorio.Obtener<ConfigComunicador>(id));
            comunicador.Dispositivo.ConcentradorId = comunicador.Dispositivo.Concentrador != null ? comunicador.Dispositivo.Concentrador.Id : 0;
            ViewBag.Sensores = ObtenerSensores(comunicador.Dispositivo.ConcentradorId);
            SetearVistaConfiguracion(drivers);
            return View(comunicador);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Comunicador)]
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

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Comunicador)]
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
                    configComunicador.PuertoDeAudio = model.PuertoDeAudio;
                    configComunicador.TiempoMaximoEjecucion = model.TiempoMaximoEjecucion;
                    configComunicador.SensorId = model.Sensor_Id;

                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Comunicador)]
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

            var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];
            var comunicador = conversor.Convertir<ConfigComunicador, ConfigComunicadorModel>(repositorio.Obtener<ConfigComunicador>(id));
            var sensor = repositorio.Obtener<Dispositivo>(x => x.Id == comunicador.Sensor_Id);
            var errores = new List<string>();

            Suscribir(sensor.Codigo, CodigosEventos.CambioEstadoIntercomunicador, urlSuscriptor, errores);
            Suscribir(sensor.Codigo, CodigosEventos.ErrorConexionDispositivo, urlSuscriptor, errores);
            Suscribir(sensor.Codigo, CodigosEventos.ConexionDispositivoCorrecta, urlSuscriptor, errores);

            var intercomunicadorConfig = GetIntercomunicadorDispositivoConfig(comunicador.Dispositivo.Codigo, sensor.Codigo, comunicador.PuertoDeAudio);
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

        [AjaxOnly]
        public ActionResult ObtenerSensoresPorConcentrador(int concentradorId)
        {
            var sensores = repositorio.Listar<ConfigSensor>(q => q.Dispositivo.Concentrador.Id == concentradorId)
                .Select(d => new SelectListItem { Text = d.Dispositivo.Descripcion, Value = d.Dispositivo.Id.ToString(CultureInfo.InvariantCulture) }).ToList();

            ViewBag.Sensores = sensores;
            return View("_SensorDeEstado");
        }

        [HttpPost]
        public ActionResult PrenderApagarDispositivo(string codigoDispositivo, bool activar)
        {
            var urlServer = new Uri(ConfigurationManager.AppSettings["ICWebServerUrl"]);
            servicio.PrenderApagarDispositivo(codigoDispositivo, activar, urlServer.Host);
            return Json(true);
        }

        private IntercomunicadorDispositivoDto GetIntercomunicadorDispositivoConfig(string codigoComunicador, string codigoSensor, int? puertoAudio)
        {
            var intercomunicadorDispositivo = new IntercomunicadorDispositivoDto
            {
                UniqueId = codigoComunicador,
                Codigo = codigoComunicador,
                Sensor = codigoSensor,
                AudioPort = (puertoAudio.HasValue) ? puertoAudio.Value.ToString() : string.Empty,
                ICPCConfig = ConfigurationManager.AppSettings["ICPCConfig"],
                ICWebServerUrl = ConfigurationManager.AppSettings["ICWebServerUrl"],
                ICWSServerUrl = ConfigurationManager.AppSettings["ICWSServerUrl"],
                DeviceActivationUrl = Url.Action("PrenderApagarDispositivo", "PruebaItc"),
                PublishingPathListen = codigoComunicador + Constantes.IntercomunicadorDireccion.HaciaLaWeb,
                PublishingPathSpeak = Constantes.IntercomunicadorDireccion.DesdeLaWeb + codigoComunicador,
            };

            return intercomunicadorDispositivo;
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

        private List<SelectListItem> ObtenerSensores(int concentradorId) {
            var sensores = repositorio.Listar<ConfigSensor>(q => q.Dispositivo.Concentrador.Id == concentradorId)
                  .Select(d => new SelectListItem { Text = d.Dispositivo.Descripcion, Value = d.Dispositivo.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            return sensores;
        }

        private void Suscribir(string codigoDisp, string codigoEvento, string urlSuscriptor, List<string> errores)
        {
            try
            {
                var resultado = servicio.Suscribir(new ComandoSuscribir
                {
                    CodigoDispositivo = codigoDisp,
                    CodigoEvento = codigoEvento,
                    RutaAccesoSuscriptor = urlSuscriptor
                });

                if (resultado.Mensaje.Codigo != 0)
                {
                    var mensaje = String.Format("{0}: {1}-{2}", codigoDisp, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
                    log.Warn(mensaje);
                    errores.Add(mensaje);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                errores.Add(String.Format("{0}: {1}", codigoDisp, Textos.PruebaItc_ErrorServicio));
            }
        }
    }
}