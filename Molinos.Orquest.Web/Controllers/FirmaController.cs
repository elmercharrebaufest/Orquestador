using System;
using System.IO;
using System.Web.Mvc;
using System.Web.UI;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    public class FirmaController : BaseController
    { 
        private ILogger log;
        private readonly IConversor conversor;

        public FirmaController(IRepositorioFactory repositorio, IConversor conversor, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            this.log = log;
            this.conversor = conversor;
        }

        [Autorizacion(PermisosOrquestador.AbmFirma)]
        public ActionResult Index()
        {
            var firma = conversor.Convertir<Firma,FirmaModel>(repositorio.Obtener<Firma>(x => true)) ?? new FirmaModel();
            return View(firma);
        }

        [HttpPost]
        [Autorizacion(PermisosOrquestador.AbmFirma)]
        public ActionResult Index(FirmaModel firmaMod)
        {
            if (ModelState.IsValid)
            {
                log.Debug("Se va a modificar datos de Empresa");
                if (firmaMod.LogoFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        firmaMod.LogoFile.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                        firmaMod.Logo = array;
                        firmaMod.LogoFile = null;
                    }
                }
                if (firmaMod.FaviconFile != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        firmaMod.FaviconFile.InputStream.CopyTo(ms);
                        byte[] array = ms.GetBuffer();
                        firmaMod.Favicon = array;
                        firmaMod.FaviconFile = null;
                    }
                }


                try
                {
                    var firma = repositorio.Obtener<Firma>(x => true);
                    if (firma == null)
                    {
                        if (firmaMod.Logo == null || firmaMod.Favicon == null)
                        {
                            throw new ArgumentException();
                        }
                        log.Debug("Creando Firma Nueva");
                        var firmaNueva = conversor.Convertir<FirmaModel, Firma>(firmaMod);
                        repositorio.Agregar(firmaNueva);
                    }
                    else
                    {
                        log.Debug("Modificando Firma");
                        conversor.Convertir(firmaMod, firma);
                    }
                    log.Debug("Se van a guardar los cambios en Firma");
                    repositorio.GuardarCambios();
                    log.Debug("Cambios guardados");
                }
                catch (ArgumentException)
                {
                    if (firmaMod.Logo == null)
                    {
                        ModelState.AddModelError("LogoFile", Textos.Logo_Requerido);
                    }
                    if (firmaMod.Favicon == null)
                    {
                        ModelState.AddModelError("Favicon", Textos.Icono_Requerido);
                    }
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", Textos.Error_ActualizarGenerico);
                    log.Error(e, Textos.Error_ActualizarGenerico);
                }
            }
            return View("Index", firmaMod);
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client)]
        public FileContentResult Logo()
        {
            var firma = repositorio.Obtener<Firma>(x => true);
            return File(firma != null && firma.Logo != null ? firma.Logo : new byte[0], "image/png");
        }

        [OutputCache(Duration = 3600, Location = OutputCacheLocation.Client)]
        public FileContentResult Favicon()
        {
            var firma = repositorio.Obtener<Firma>(x => true);
            return File(firma != null && firma.Favicon != null ? firma.Favicon : new byte[0], "image/x-icon");
        }
    }
}