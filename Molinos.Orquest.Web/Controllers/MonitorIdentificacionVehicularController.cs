using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Ninject.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.Net.Mime;
using System.Security.Principal;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.MonitorIdentificacionVehicular)]
    public class MonitorIdentificacionVehicularController : BaseController
    {
        public MonitorIdentificacionVehicularController(
            IRepositorioFactory repositorio,
            IServicioOrquestador servicio,
            ILogger log)
            : base(repositorio, servicio, log)
        {
        }

        public ActionResult Index(string codigoCIV)
        {
            try
            {
                var config = repositorio.Obtener<ConfigIdentificacionVehicular>(c => c.Codigo == codigoCIV);

                if (config == null)
                    return HttpNotFound();

                try
                {
                    ViewBag.EstadoDispositivos = servicio.ObtenerEstadoDispositivosCIV(codigoCIV);
                }
                catch (Exception ex)
                {
                    log.Warn(ex, "No se pudo obtener estado de dispositivos CIV (servicio no disponible): {0}", codigoCIV);
                    ViewBag.EstadoDispositivos = new System.Collections.Generic.List<Molinos.Orquest.Dominio.Dtos.EstadoDispositivoCIVDto>();
                    ViewBag.ServicioNoDisponible = true;
                }

                return View(config);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Error al cargar Monitor CIV: {0}", codigoCIV);
                ViewBag.Mensaje = $"Error al comunicarse con el servicio: {ex.Message}";
                ViewBag.DetalleError = ex.ToString();
                return View("Error");
            }
        }

        [HttpGet]
        public ActionResult ObtenerEstadoDispositivos(string codigoCIV)
        {
            var estado = servicio.ObtenerEstadoDispositivosCIV(codigoCIV);
            return Json(estado, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ObtenerFoto(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
                return HttpNotFound();

            try
            {
                log.Debug("ObtenerFoto: usuario proceso={0}", WindowsIdentity.GetCurrent().Name);
                log.Debug("ObtenerFoto: ruta recibida={0}", ruta);

                var rutaArchivoNorm = NormalizarRutaUNC(ruta);

                log.Debug("ObtenerFoto: ruta normalizada={0}", rutaArchivoNorm);

                if (!Path.IsPathRooted(rutaArchivoNorm))
                {
                    log.Warn("ObtenerFoto: ruta no absoluta rechazada. ruta={0}", ruta);
                    return new HttpStatusCodeResult(403, "Ruta no absoluta.");
                }

                var rutaFotos = ConfigurationManager.AppSettings["IdentificacionVehicular.RutaFotos"];

                if (!string.IsNullOrWhiteSpace(rutaFotos))
                {
                    var rutaFotosNorm = NormalizarRutaUNC(rutaFotos)
                        .TrimEnd('\\', '/') + "\\";

                    var rutaArchivoParaComparar = rutaArchivoNorm;

                    if (!rutaArchivoParaComparar.StartsWith(rutaFotosNorm, StringComparison.OrdinalIgnoreCase))
                    {
                        log.Warn(
                            "ObtenerFoto: ruta fuera del directorio permitido. rutaBase={0} ruta={1}",
                            rutaFotosNorm,
                            rutaArchivoParaComparar
                        );

                        return new HttpStatusCodeResult(403, "Ruta fuera del directorio permitido.");
                    }
                }

                var directorio = Path.GetDirectoryName(rutaArchivoNorm);

                log.Debug("ObtenerFoto: directorio={0}", directorio);
                log.Debug("ObtenerFoto: Directory.Exists={0}", Directory.Exists(directorio));

                var existeArchivo = System.IO.File.Exists(rutaArchivoNorm);

                log.Debug("ObtenerFoto: File.Exists={0}", existeArchivo);

                if (!existeArchivo)
                {
                    return new HttpStatusCodeResult(
                        404,
                        "Archivo no encontrado o el usuario del AppPool no tiene permisos."
                    );
                }

                try
                {
                    using (var fs = new FileStream(
                        rutaArchivoNorm,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        log.Debug("ObtenerFoto: archivo abierto correctamente. bytes={0}", fs.Length);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    log.Error("ObtenerFoto: sin permisos para leer archivo. ruta=" + rutaArchivoNorm, ex);
                    return new HttpStatusCodeResult(403, "Sin permisos para leer la imagen.");
                }

                var extension = Path.GetExtension(rutaArchivoNorm)
                    .TrimStart('.')
                    .ToLowerInvariant();

                var contentType = extension == "jpg" || extension == "jpeg"
                    ? MediaTypeNames.Image.Jpeg
                    : "image/" + extension;

                return File(rutaArchivoNorm, contentType);
            }
            catch (Exception ex)
            {
                log.Error("ObtenerFoto: error general. ruta=" + ruta, ex);
                return new HttpStatusCodeResult(500, "Error accediendo a la imagen: " + ex.Message);
            }
        }

        /// <summary>
        /// Normaliza una ruta eliminando separadores duplicados, preservando el prefijo UNC (\\).
        /// </summary>
        private static string NormalizarRutaUNC(string ruta)
        {
            if (string.IsNullOrEmpty(ruta))
                return ruta;

            bool esUNC = ruta.StartsWith(@"\\");

            // Reemplazar todas las barras diagonales por backslash y colapsar múltiples barras
            var normalizada = System.Text.RegularExpressions.Regex.Replace(
                ruta.Replace('/', Path.DirectorySeparatorChar),
                @"\\{2,}",
                m => m.Index == 0 ? @"\\" : @"\");

            // Restaurar el prefijo UNC si fue colapsado
            if (esUNC && !normalizada.StartsWith(@"\\"))
                normalizada = @"\\" + normalizada.TrimStart('\\');

            return normalizada;
        }
    }
}
