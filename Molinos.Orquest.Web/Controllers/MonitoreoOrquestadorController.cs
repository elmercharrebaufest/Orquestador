using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.MonitoreoOrquestador)]
    public class MonitoreoOrquestadorController : BaseController
    {
        public MonitoreoOrquestadorController(IRepositorioFactory repositorio,IServicioOrquestador servicio, ILogger log ): base(repositorio,servicio,log)
        {
        }
        public ActionResult Index()
        {
            SetearVista();
            return View();
        }

        [HttpGet]
        public ActionResult Iniciar(string server)
        {
            servicio.IniciarServiceOrquestador(server);
            return Json("El servicio se ha iniciado", JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Detener(string server)
        {
            log.Debug("Detener Service");
            servicio.DetenerServiceOrquestador(server);
            return Json("El servicio se ha detenido", JsonRequestBehavior.AllowGet);
        }

        private void SetearVista()
        {
            var elem = ConfigurationManager.AppSettings["HostServiciosWeb"];
            var list = elem.Split(';').ToList();
            ViewBag.Servers = list.Select(x => new MonitoreoOrquestadorDto { Maquina = x/*,EstadoServicio = EstadoActualServicio(x)*/}).ToList();
        }

        
        //public string EstadoActualServicio(string server)
        //{

        //    return servicio.ObtenerEstadoServiceOrquestador(server);
        //}
    }
}
