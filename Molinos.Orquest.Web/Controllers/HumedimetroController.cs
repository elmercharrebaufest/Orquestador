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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.Humedimetro, PermisosOrquestador.PruebaHumedimetro)]
    public class HumedimetroController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public HumedimetroController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverHumedimetro>();
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
            Expression<Func<ConfigHumedimetro, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigHumedimetro, ConfigHumedimetroModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.Humedimetro)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Humedimetro)]
        public ActionResult Crear(ConfigHumedimetroModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    model.DelimitadorCampos = Server.UrlDecode(model.DelimitadorCampos);
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigHumedimetroModel, ConfigHumedimetro>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Humedimetro)]
        public ActionResult Modificar(int id)
        {
            var humedimetro = conversor.Convertir<ConfigHumedimetro, ConfigHumedimetroModel>(repositorio.Obtener<ConfigHumedimetro>(id));
            humedimetro.Dispositivo.ConcentradorId = humedimetro.Dispositivo.Concentrador != null ? humedimetro.Dispositivo.Concentrador.Id : 0;
            humedimetro.DelimitadorCampos = CaracterValido(humedimetro.DelimitadorCampos);
            SetearVistaConfiguracion(drivers);
            return View(humedimetro);
        }

        private string CaracterValido(string delimitadorCampos)
        {
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(delimitadorCampos.ToCharArray());
            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            if (asciiChars.Length > 0)
            {
                if (asciiChars[0] >= 0 && asciiChars[0] <= 31)
                {
                    return Server.UrlEncode(delimitadorCampos);
                }
            }
            return delimitadorCampos;
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Humedimetro)]
        public ActionResult Modificar(ConfigHumedimetroModel model)
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
                    var configHumedimetro = (ConfigHumedimetro)viejo.Configuracion;
                    configHumedimetro.ComandoHumedad = model.ComandoHumedad;
                    configHumedimetro.DelimitadorCampos = Server.UrlDecode(model.DelimitadorCampos);
                    configHumedimetro.LongFrase = model.LongFrase;
                    configHumedimetro.PosicionCampoHumedad = model.PosicionCampoHumedad;
                    configHumedimetro.PosicionCampoPesoHectolitrico = model.PosicionCampoPesoHectolitrico;
                    configHumedimetro.DireccionIp = model.DireccionIp;
                    configHumedimetro.LongFrase = model.LongFrase;
                    configHumedimetro.Puerto = model.Puerto;
                    configHumedimetro.TimeoutLectura = model.TimeoutLectura;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Humedimetro)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var cabezal = repositorio.Obtener<ConfigHumedimetro>(id);
            if (cabezal.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarHumedimetroPorOrquestado);
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
            var humedimetro = conversor.Convertir<ConfigHumedimetro, ConfigHumedimetroModel>(repositorio.Obtener<ConfigHumedimetro>(id));
            return View(humedimetro);
        }

        public ActionResult ObtenerHumedad(string codigo, long fecha)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarAnalisisHumedad { CodigoDispositivo = codigo, FechaDeInicio = new DateTime(fecha) });

                if (resultado.Mensaje.Codigo == Codigos.OK)
                {
                    if (resultado.Valores.ContainsKey("AnalisisHumedad"))
                    {
                        resultados.Add(new ResultadoPruebaModel("Humedad: " + resultado.Valores["AnalisisHumedad"].ToString(CultureInfo.CurrentUICulture), false));
                    }

                    if (resultado.Valores.ContainsKey("PH"))
                    {
                        resultados.Add(new ResultadoPruebaModel("PH: " + resultado.Valores["PH"].ToString(CultureInfo.CurrentUICulture), false));
                    }
                }
                else
                {
                    resultados.Add(new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error al obtener el análisis de humedad del dispositivo {0}", codigo);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }

            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        public JsonResult ConsultarEstado(string codigoDispositivo)
        {
            try
            {
                log.Debug("Consultando estado Humedimetro {0}", codigoDispositivo);
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