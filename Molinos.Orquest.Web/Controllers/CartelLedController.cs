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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.CartelLed)]
    public class CartelLedController : BaseController
    {
        // GET: CartelLed
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public CartelLedController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverCartelLed>();
        }
       

        public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            FillViewBag();
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
            Expression<Func<ConfigCartelLed, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) /*|| x.ClaseDriver.Contains(filtro)*/;
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigCartelLed, ConfigCartelLedModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        
        public ActionResult Crear()
        {           
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.CartelLed)]
        public ActionResult Crear(ConfigCartelLedModel model)
        {
          
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    if (!model.Dispositivo.EsConcentrador && model.Dispositivo.ConcentradorId > 0)
                    {
                        model.Dispositivo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    }
                    repositorio.Agregar(conversor.Convertir<ConfigCartelLedModel, ConfigCartelLed>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }
        [Autorizacion(PermisosOrquestador.CartelLed)]
        public ActionResult Modificar(int id)
        {
            var cartel = conversor.Convertir<ConfigCartelLed, ConfigCartelLedModel>(repositorio.Obtener<ConfigCartelLed>(id));
            cartel.Dispositivo.ConcentradorId = cartel.Dispositivo.Concentrador != null ? cartel.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(cartel);
        }
        protected void SetearVistaConfiguracion(IEnumerable<string> drivers, bool puedeSerConcentrador = false)
        {
            FillViewBag();
            ViewBag.PuedeSerConcentrador = puedeSerConcentrador;
            ViewBag.Drivers = drivers.Select(d => new SelectListItem { Text = d, Value = d }).ToList();
            var concentradores = repositorio.Listar<Dispositivo>(w => w.EsConcentrador).Select(d => new SelectListItem { Text = d.Descripcion, Value = d.Id.ToString(CultureInfo.InvariantCulture) }).ToList();
            concentradores.Insert(0, new SelectListItem { Selected = true, Text = Textos.NoTiene, Value = "0" });
            ViewBag.Concentradores = concentradores;
        }
        [HttpPost]
        public ActionResult Modificar(ConfigCartelLedModel model)
        {
            FillViewBag();
            if (ModelState.IsValid)
            {
                if (ValidacionesDeNegocio(model.Dispositivo))
                {
                    var viejo = repositorio.Obtener<Dispositivo>(model.Id);
                    viejo.Codigo = model.Dispositivo.Codigo;
                    viejo.Descripcion = model.Dispositivo.Descripcion;
                    viejo.Activo = model.Dispositivo.Activo;
                    viejo.EsConcentrador = model.Dispositivo.EsConcentrador;
                    viejo.Concentrador = repositorio.Obtener<Dispositivo>(model.Dispositivo.ConcentradorId);
                    var conf = (ConfigCartelLed)viejo.Configuracion;
                    conf.DireccionIp = model.DireccionIp;
                    conf.Puerto = model.Puerto;
                    conf.ControlBrillo = model.ControlBrillo;
                    conf.Efecto = model.Efecto;
                    conf.Tipografia = model.Tipografia;
                    conf.VelocidadScroll = model.VelocidadScroll;
                    conf.TimeoutLectura = model.TimeoutLectura;
                    conf.LongFrase = model.LongFrase;
                    conf.NumeroTrama = model.NumeroTrama;
                    conf.NumeroVariable = model.NumeroVariable;
                    conf.NumeroPrograma = model.NumeroPrograma;
                    conf.ClaseDriver = model.ClaseDriver;
                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.CartelLed)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var cartel = repositorio.Obtener<ConfigCartelLed>(id);
            if (cartel.Dispositivo.TomadoPor != null)
            {
                return Content(Textos.Error_Eliminar_Cartel_Led);
            }
            repositorio.Remover(cartel.Dispositivo);
            repositorio.Remover(cartel);
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
            var cartel =  conversor.Convertir<ConfigCartelLed, ConfigCartelLedModel>(repositorio.Obtener<ConfigCartelLed>(id));
            return View("Probar", cartel);
        }

        public ActionResult Ejecutar(string codigo, string texto, string numeroPrograma, string numeroTrama, string numeroVariable)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarEnviarMensaje { 
                    CodigoDispositivo = codigo, 
                    Texto = texto,
                    NumeroPrograma = numeroPrograma,
                NumeroTrama = numeroTrama,
                NumeroVariable= numeroVariable
                });
                if (resultado.Mensaje.Codigo == Codigos.OK)
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

        private void FillViewBag()
        {
            var brillo = new List<SelectListItem>() {
                new SelectListItem { Text= "Control Automático", Value= "222", Selected = false },
                new SelectListItem { Text = "Brillo Mínimo", Value = "100", Selected = false },
                new SelectListItem { Text = "Brillo Medio", Value = "110", Selected = false },
                new SelectListItem { Text = "Brillo Máximo", Value = "120", Selected = false }
                };
            ViewBag.Brillo = brillo;

            var efecto = new List<SelectListItem>() {
                new SelectListItem { Text= "Ninguno", Value= "100", Selected = false },
                new SelectListItem { Text = "Titila Rápido", Value = "102", Selected = false },
                new SelectListItem { Text = "Titila Lento", Value = "104", Selected = false },
                new SelectListItem { Text = "Negativo", Value = "108", Selected = false },
                new SelectListItem { Text = "Titila Negativo", Value = "106", Selected = false },
                new SelectListItem { Text = "Aparece Arriba", Value = "114", Selected = false },
                new SelectListItem { Text = "Aparece Abajo", Value = "116", Selected = false },
                new SelectListItem { Text = "Cortina", Value = "118", Selected = false }
            };
            ViewBag.Efectoid = efecto;

            var tipografia = new List<SelectListItem>() {
                new SelectListItem { Text= "6x5 Simple", Value= "175", Selected = false },
                new SelectListItem { Text = "6x4 Simple", Value = "195", Selected = false },
                new SelectListItem { Text = "5x8 Doble", Value = "185", Selected = false },
                new SelectListItem { Text = "7x8 Doble", Value = "215", Selected = false },
                new SelectListItem { Text = "7x8 Media", Value = "225", Selected = false },
                new SelectListItem { Text = "7x11 Doble", Value = "205", Selected = false },
                new SelectListItem { Text = "7x6 Doble", Value = "255", Selected = false },
                new SelectListItem { Text = "7x6 Simple", Value = "235", Selected = false }
            };
            ViewBag.Tipografiaid = tipografia;
        }
    }
}