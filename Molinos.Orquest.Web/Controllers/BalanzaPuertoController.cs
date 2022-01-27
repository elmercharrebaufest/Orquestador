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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
    public class BalanzaPuertoController : BaseController
    {
        private readonly IConversor conversor;
        private readonly IEnumerable<string> drivers;

        public BalanzaPuertoController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IConversor conversor, IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
            drivers = driverFactory.DriversDisponibles<IDriverBalanzaPuerto>();
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
            Expression<Func<ConfigBalanzaPuerto, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Dispositivo.Codigo.Contains(filtro) || x.Dispositivo.Descripcion.Contains(filtro) || x.DireccionIp.Contains(filtro) || x.ClaseDriver.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            var consulta = conversor.ConvertirListaPaginada<ConfigBalanzaPuerto, ConfigBalanzaPuertoModel>(repositorio.Listar(expresionFiltro, paginacion));
            ViewBag.Items = consulta;
        }

        [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
        public ActionResult Crear()
        {
            SetearVistaConfiguracion(drivers);
            return View();
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
        public ActionResult Crear(ConfigBalanzaPuertoModel model)
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
                    repositorio.Agregar(conversor.Convertir<ConfigBalanzaPuertoModel, ConfigBalanzaPuerto>(model));
                    repositorio.GuardarCambios();
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
        public ActionResult Modificar(int id)
        {
            var cabezal = conversor.Convertir<ConfigBalanzaPuerto, ConfigBalanzaPuertoModel>(repositorio.Obtener<ConfigBalanzaPuerto>(id));
            cabezal.Dispositivo.ConcentradorId = cabezal.Dispositivo.Concentrador != null ? cabezal.Dispositivo.Concentrador.Id : 0;
            SetearVistaConfiguracion(drivers);
            return View(cabezal);
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
        [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
        public ActionResult Modificar(ConfigBalanzaPuertoModel model)
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
                    var configCabezal = (ConfigBalanzaPuerto)viejo.Configuracion;
                    configCabezal.DireccionIp = model.DireccionIp;
                    configCabezal.PosDesde = model.PosDesde;
                    configCabezal.PosHasta = model.PosHasta;
                    configCabezal.Puerto = model.Puerto;
                    configCabezal.TimeoutLectura = model.TimeoutLectura;
                    configCabezal.LongFrase = model.LongFrase;
                    configCabezal.IntervaloPolling = model.IntervaloPolling;
                    configCabezal.ComandoConsulta = model.ComandoConsulta;
                    configCabezal.ComandoBorrado = model.ComandoBorrado;
                    configCabezal.CantidadCaracteresTotal = model.CantidadCaracteresTotal;
                    configCabezal.CaracterIzquierdaACompletar = model.CaracterIzquierdaACompletar;

                    repositorio.GuardarCambios();
                    RecargarConfiguracion(model.Dispositivo.Codigo);
                    return new AjaxEditSuccessResult();
                }
            }
            SetearVistaConfiguracion(drivers);
            return View(model);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.BalanzaPuerto)]
        public ActionResult Eliminar(int id)
        {
            var contenido = "true";
            var cabezal = repositorio.Obtener<ConfigBalanzaPuerto>(id);
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
            var cabezal = conversor.Convertir<ConfigBalanzaPuerto, ConfigBalanzaPuertoModel>(repositorio.Obtener<ConfigBalanzaPuerto>(id));
            return View(cabezal);
        }


        public ActionResult ConsultaBalanzada(string codigo, int? idBalanzada)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarConsultaBalanzada { CodigoDispositivo = codigo, IdBalanzada = idBalanzada });

                if (resultado.Mensaje.Codigo == Codigos.OK)
                {
                    foreach (var r in ((ResultadoConsultaBalanzada)resultado).ValoresBalanzada)
                    {
                        resultados.Add(new ResultadoPruebaModel(r.Key + ": " + r.Value, false));
                    }
                }
                else
                {
                    resultados.Add(new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error al obtener la balanzada {0}", idBalanzada);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }

            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        public ActionResult ConsultaBalanzadaPorRango(string codigo, int idBalanzadaInicio, int idBalanzadaFin)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = servicio.Ejecutar(new EjecutarConsultaBalanzadaPorRango { CodigoDispositivo = codigo, IdBalanzadaInicio = idBalanzadaInicio, IdBalanzadaFin = idBalanzadaFin });
                foreach (var b in ((ResultadoConsultaBalanzadaPorRango)resultado).Balanzadas)
                {
                    if (b.Mensaje.Codigo == Codigos.OK)
                    {
                        foreach (var r in b.ValoresBalanzada)
                        {
                            resultados.Add(new ResultadoPruebaModel(r.Key + ": " + r.Value, false));
                        }
                        resultados.Add(new ResultadoPruebaModel("--------------------", false));
                    }
                    else
                    {
                        resultados.Add(new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
                        resultados.Add(new ResultadoPruebaModel("--------------------", true));
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e, "Error al obtener la balanzada {0}", idBalanzadaInicio);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }

            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        public ActionResult BorradoBalanzada(string codigo, int? idBalanzadaBorrado)
        {            
            var resultados = new List<ResultadoPruebaModel>();

            try
            {
                var resultado = (ResultadoBorrarBalanzada)servicio.Ejecutar(new EjecutarBorrarBalanzada { CodigoDispositivo = codigo, IdBorrado = idBalanzadaBorrado.Value });
                resultados.Add(resultado.Mensaje.Codigo == Codigos.OK
                                   ? new ResultadoPruebaModel(resultado.Mensaje.ToString(), false)
                                   : new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
            }
            catch (Exception e)
            {
                log.Error(e, "Error al eliminar la balanzada {0}", codigo);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }


            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

        public ActionResult BorradoBalanzadaPorRango(string codigo, int? idBalanzadaBorradoInicio, int? idBalanzadaBorradoFin)
        {
            var resultados = new List<ResultadoPruebaModel>();
            try
            {
                var resultado = (ResultadoBorrarBalanzadasPorRango)servicio.Ejecutar(new EjecutarBorrarBalanzadasPorRango { CodigoDispositivo = codigo, IdBalanzadaInicio = idBalanzadaBorradoInicio.Value, IdBalanzadaFin = idBalanzadaBorradoFin.Value });
                resultados.Add(resultado.Mensaje.Codigo == Codigos.OK
                                   ? new ResultadoPruebaModel(resultado.ToString(), false)
                                   : new ResultadoPruebaModel(resultado.Mensaje.ToString(), true));
            }
            catch (Exception e)
            {
                log.Error(e, "Error al eliminar la balanzada {0}", codigo);
                resultados.Add(new ResultadoPruebaModel(Textos.PruebaItc_ErrorServicio, true));
            }

            return View("~/Views/PruebaConexion/ResultadoPrueba.cshtml", resultados);
        }

    }
}