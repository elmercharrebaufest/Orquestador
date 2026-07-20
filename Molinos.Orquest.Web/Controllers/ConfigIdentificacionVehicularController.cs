using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Molinos.Orquest.Dominio;
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
    [Autorizacion(PermisosOrquestador.ConfigIdentificacionVehicular)]
    public class ConfigIdentificacionVehicularController : BaseController
    {
        private readonly IConversor conversor;

        public ConfigIdentificacionVehicularController(IRepositorioFactory repositorio, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.conversor = conversor;
        }

        public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)
        {
            ListQuery(filtro, pagina, ordenarPor, dirOrden);
            SuscribirCIVsActivas();
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
            Expression<Func<ConfigIdentificacionVehicular, bool>> expresionFiltro = null;
            if (!string.IsNullOrEmpty(filtro))
            {
                filtro = filtro.Trim();
                expresionFiltro = x => x.Nombre.Contains(filtro) || x.Codigo.Contains(filtro);
            }
            var paginacion = new Paginacion(ordenarPor, dirOrden, pagina, 8);
            ViewBag.Items = repositorio.Listar(expresionFiltro, paginacion);
        }

        public ActionResult Crear()
        {
            PopularLectoresYSensores();
            return View();
        }

        [HttpPost]
        public ActionResult Crear(ConfigIdentificacionVehicularModel model, int[] camaraIds)
        {
            ValidarDispositivo(model);
            if (ModelState.IsValid)
            {
                var entidad = new ConfigIdentificacionVehicular
                {
                    Nombre = model.Nombre,
                    Codigo = model.Codigo,
                    ConfigLectorTarjetasId = model.ConfigLectorTarjetasId,
                    ConfigSensorVehicularId = model.ConfigSensorVehicularId,
                    ConfigSensorPresenciaId = model.ConfigSensorPresenciaId,
                    MaxReintentosFoto = model.MaxReintentosFoto,
                    DelayEntreReintentosMs = model.DelayEntreReintentosMs,
                    Activo = model.Activo,
                    Camaras = BuildCamaras(camaraIds)
                };
                repositorio.Agregar(entidad);
                repositorio.GuardarCambios();
                servicio.RecargarConfigIdentificacionVehicular(entidad.Codigo);
                return RedirectToAction("Index");
            }
            PopularLectoresYSensores();
            return View(model);
        }

        public ActionResult Modificar(int id)
        {
            var config = repositorio.Obtener<ConfigIdentificacionVehicular>(id);
            var model = conversor.Convertir<ConfigIdentificacionVehicular, ConfigIdentificacionVehicularModel>(config);
            PopularLectoresYSensores();
            return View(model);
        }

        [HttpPost]
        public ActionResult Modificar(ConfigIdentificacionVehicularModel model, int[] camaraIds)
        {
            ValidarDispositivo(model);
            if (ModelState.IsValid)
            {
                var entidad = repositorio.Obtener<ConfigIdentificacionVehicular>(model.Id);
                entidad.Nombre = model.Nombre;
                entidad.Codigo = model.Codigo;
                entidad.ConfigLectorTarjetasId = model.ConfigLectorTarjetasId;
                entidad.ConfigSensorVehicularId = model.ConfigSensorVehicularId;
                entidad.ConfigSensorPresenciaId = model.ConfigSensorPresenciaId;
                entidad.MaxReintentosFoto = model.MaxReintentosFoto;
                entidad.DelayEntreReintentosMs = model.DelayEntreReintentosMs;
                entidad.Activo = model.Activo;

                foreach (var camara in entidad.Camaras.ToList())
                    repositorio.Remover(camara);

                foreach (var camara in BuildCamaras(camaraIds))
                    entidad.Camaras.Add(camara);

                repositorio.GuardarCambios();
                servicio.RecargarConfigIdentificacionVehicular(entidad.Codigo);
                return RedirectToAction("Index");
            }
            PopularLectoresYSensores();
            return View(model);
        }

        [HttpPost]
        public ActionResult Eliminar(int id)
        {
            var config = repositorio.Obtener<ConfigIdentificacionVehicular>(id);
            foreach (var camara in config.Camaras.ToList())
                repositorio.Remover(camara);

            repositorio.Remover(config);
            try
            {
                repositorio.GuardarCambios();
            }
            catch (EntidadReferenciadaException)
            {
                if (Request.IsAjaxRequest())
                    return Content(Textos.Error_EliminarReferenciado);
            }
            if (Request.IsAjaxRequest())
                return Content("true");

            return RedirectToAction("Index");
        }

        private void PopularLectoresYSensores()
        {
            var lectores = repositorio.Listar<ConfigLectorTarjetas>()
                .Select(x => new SelectListItem
                {
                    Text = x.Dispositivo.Descripcion + " (" + x.Dispositivo.Codigo + ")",
                    Value = x.Id.ToString(CultureInfo.InvariantCulture)
                }).ToList();
            lectores.Insert(0, new SelectListItem { Text = "— Seleccione un lector —", Value = "" });

            var sensoresVehiculares = repositorio.Listar<ConfigSensor>(x => x.ClaseDriver == Constantes.Drivers.DriverLectorPatente)
                .Select(x => new SelectListItem
                {
                    Text = x.Dispositivo.Descripcion + " (" + x.Dispositivo.Codigo + ")",
                    Value = x.Id.ToString(CultureInfo.InvariantCulture)
                }).ToList();
            sensoresVehiculares.Insert(0, new SelectListItem { Text = "— Seleccione un sensor —", Value = "" });

            ViewBag.Lectores = lectores;
            ViewBag.SensoresVehiculares = sensoresVehiculares;
            ViewBag.AvailableCameras = repositorio.Listar<ConfigCamara>()
                .Select(x => new ConfigIdentificacionVehicularModel.CamaraItemModel
                {
                    Id = x.Id,
                    Nombre = x.Dispositivo.Descripcion + " (" + x.Dispositivo.Codigo + ")",
                    Ip = x.Uri ?? ""
                })
                .ToList();
        }

        private ICollection<ConfigIdentificacionVehicularCamara> BuildCamaras(int[] camaraIds)
        {
            var camaras = new List<ConfigIdentificacionVehicularCamara>();
            if (camaraIds == null || camaraIds.Length == 0) return camaras;

            foreach (var id in camaraIds)
                camaras.Add(new ConfigIdentificacionVehicularCamara { ConfigCamaraId = id });

            return camaras;
        }

        private void ValidarDispositivo(ConfigIdentificacionVehicularModel model)
        {
            if (model.Activo && !model.ConfigLectorTarjetasId.HasValue && !model.ConfigSensorVehicularId.HasValue)
            {
                ModelState.AddModelError("ConfigLectorTarjetasId",
                    Textos.ConfigIdentificacionVehicular_DebeSeleccionarDispositivo);
            }

            if (repositorio.Existe<ConfigIdentificacionVehicular>(
                e => e.Codigo == model.Codigo && (model.Id == 0 || e.Id != model.Id)))
            {
                ModelState.AddModelError("Codigo", Textos.Camara_CodigoExistente);
            }
        }

        private void SuscribirCIVsActivas()
        {
            var urlSuscriptor = ConfigurationManager.AppSettings["UrlServicioSuscriptor"];
            if (string.IsNullOrEmpty(urlSuscriptor))
                return;

            var civsActivas = repositorio.Listar<ConfigIdentificacionVehicular>(c => c.Activo);
            foreach (var civ in civsActivas)
            {
                try
                {
                    servicio.SuscribirIdentificacionVehicular(civ.Codigo, CodigosEventos.IdentificacionVehicular, urlSuscriptor);
                }
                catch (Exception ex)
                {
                    log.Warn(ex, "No se pudo suscribir al evento IdentificacionVehicular para CIV '{0}'.", civ.Codigo);
                }
            }
        }
    }
}
