using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
    [Autorizacion(PermisosOrquestador.Cabezal, PermisosOrquestador.PruebaCabezal)]
    public class CabezalController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public CabezalController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log):base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            try
            {
                drivers = driverFactory.DriversDisponibles<IDriverCabezal>();

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
                log.Error(ex,errorMessage);
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
            Expression<Func<ConfigCabezal, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigCabezal, ConfigCabezalModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.Cabezal)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Cabezal)]
        public ActionResult Crear(ConfigCabezalModel model)
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
                    model.CarInicioFrase = Server.UrlDecode(model.CarInicioFrase);
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigCabezalModel, ConfigCabezal>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Cabezal)]
        public ActionResult Modificar(int id)
        {
            var cabezal = conversor.Convertir<ConfigCabezal, ConfigCabezalModel>(repositorio.Obtener<ConfigCabezal>(id));
            cabezal.Dispositivo.ConcentradorId = cabezal.Dispositivo.Concentrador != null ? cabezal.Dispositivo.Concentrador.Id : 0;
            cabezal.CarInicioFrase = String.Join(",", cabezal.CarInicioFrase.Split(',').Select(CaracterValido));
            SetearVistaConfiguracion(drivers);
            return View(cabezal);
        }

        private string CaracterValido(string carInicioFrase)
        {
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(carInicioFrase.ToCharArray()); 
            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            if (asciiChars.Length > 0){
                if (asciiChars[0] >= 0 && asciiChars[0] <= 31)
                {
                    return Server.UrlEncode(carInicioFrase);
                }
            }
            return carInicioFrase;
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Cabezal)]
        public ActionResult Modificar(ConfigCabezalModel model)
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
                    var configCabezal = (ConfigCabezal) viejo.Configuracion;
                    configCabezal.CantLecPesoEstable = model.CantLecPesoEstable;
                    configCabezal.CarInicioFrase = Server.UrlDecode(model.CarInicioFrase);
                    configCabezal.ComandoCereo = model.ComandoCereo;
                    configCabezal.ComandoPeso = model.ComandoPeso;
                    configCabezal.DireccionIp = model.DireccionIp;
                    configCabezal.IntLecCereo = model.IntLecCereo;
                    configCabezal.IntLecPesoEstable = model.IntLecPesoEstable;
                    configCabezal.LongFrase = model.LongFrase;
                    configCabezal.MaxCantLecPesoEstable = model.MaxCantLecPesoEstable;
                    configCabezal.PosDesde = model.PosDesde;
                    configCabezal.PosHasta = model.PosHasta;
                    configCabezal.Puerto = model.Puerto;
                    configCabezal.TimeoutLectura = model.TimeoutLectura;
                    configCabezal.DigitosDecimales = model.DigitosDecimales;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Cabezal)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var cabezal = repositorio.Obtener<ConfigCabezal>(id);
            if (cabezal.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarCabezalPorOrquestado);
            }
            repositorio.Remover(cabezal.Dispositivo);
            repositorio.Remover(cabezal);
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
            var cabezal = conversor.Convertir<ConfigCabezal, ConfigCabezalModel>(repositorio.Obtener<ConfigCabezal>(id));
            return View(cabezal);
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


    }
}