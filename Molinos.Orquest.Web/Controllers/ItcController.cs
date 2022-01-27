using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
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
    [Autorizacion(PermisosOrquestador.Itc, PermisosOrquestador.PruebaItc)]
    public class ItcController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public ItcController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverItc>();
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
            Expression<Func<ConfigItc, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigItc, ConfigItcModel>(repositorio.Listar(expresionFiltro, paginacion));

            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers, true);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Crear(ConfigItcModel model)
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
                    model.CarFinFrase = Server.UrlDecode(model.CarFinFrase);
                    model.DelimitadorCampos = Server.UrlDecode(model.DelimitadorCampos);
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigItcModel, ConfigItc>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers, true);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Modificar(int id)
        {
            var itc = conversor.Convertir<ConfigItc, ConfigItcModel>(repositorio.Obtener<ConfigItc>(id));
            itc.Dispositivo.ConcentradorId = itc.Dispositivo.Concentrador != null ? itc.Dispositivo.Concentrador.Id : 0;
            itc.CarInicioFrase = CaracterValido(itc.CarInicioFrase);
            itc.CarFinFrase = CaracterValido(itc.CarFinFrase);
            itc.DelimitadorCampos = CaracterValido(itc.DelimitadorCampos);
            SetearVistaConfiguracion(drivers, true);
            return View(itc);
        }

        private string CaracterValido(string caracter)
        {
            var ascii = Encoding.ASCII;
            var asciiBytes = ascii.GetBytes(caracter.ToCharArray()); 
            var asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
            ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
            if (asciiChars.Length > 0){
                if (asciiChars[0] >= 0 && asciiChars[0] <= 31)
                {
                    return Server.UrlEncode(caracter);
                }
            }
            return caracter;
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Modificar(ConfigItcModel model)
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
                    var configItc = (ConfigItc) viejo.Configuracion;
                    configItc.CarFinFrase = Server.UrlDecode(model.CarFinFrase);
                    configItc.CarInicioFrase = Server.UrlDecode(model.CarInicioFrase);
                    configItc.ComandoActivarSalida = model.ComandoActivarSalida;
                    configItc.ComandoEstado = model.ComandoEstado;
                    configItc.ComandoTarjeta = model.ComandoTarjeta;
                    configItc.DireccionIp = model.DireccionIp;
                    configItc.DelimitadorCampos = Server.UrlDecode(model.DelimitadorCampos);
                    configItc.IntervaloPolling = model.IntervaloPolling;
                    configItc.LongFrase = model.LongFrase;
                    configItc.RespuestaError = model.RespuestaError;
                    configItc.RespuestaExito = model.RespuestaExito;
                    configItc.Puerto = model.Puerto;
                    configItc.TimeoutLectura = model.TimeoutLectura;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers, true);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.Itc)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var itc = repositorio.Obtener<ConfigItc>(id);
            if (itc.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_EliminarItcPorOrquestado);
            }
            repositorio.Remover(itc.Dispositivo);
            repositorio.Remover(itc);
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
            return RedirectToAction("Index", "PruebaItc", new {id});
        }

    }
}