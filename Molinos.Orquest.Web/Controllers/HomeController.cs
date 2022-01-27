using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Conversiones;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IdentityModel.Services;
using System.Linq;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    public class HomeController : BaseController
    {
     

        public HomeController(IRepositorioFactory repositorio, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
           
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult BorrarPermisosCookie()
        {
            if (FederatedAuthentication.SessionAuthenticationModule != null)
            {

                FederatedAuthentication.SessionAuthenticationModule.DeleteSessionTokenCookie();
            }

            return RedirectToAction("Index", "Home");
        }
        public JsonResult ListarDispositivos()
        {
            var estado = repositorio.Listar<Estado>(x => true).FirstOrDefault();

            var dispositivos = repositorio.Listar<ConfigDispositivo, DispositivoDto>(x=> x.Dispositivo.Concentrador == null, x=> new DispositivoDto {
                Codigo = x.Dispositivo.Codigo,
                Activo = x.Dispositivo.Activo,
                Descripcion = x.Dispositivo.Descripcion,
                ClaseDriver = x.ClaseDriver,
                EstadoCorrecto = x.Dispositivo.EstadoCorrecto,
                EsConcentrador = x.Dispositivo.EsConcentrador
            }).OrderBy(x => x.ClaseDriver);

            var dispositivosVirtual = repositorio.Listar<ConfigDispositivo, DispositivoDto>(x => x.Dispositivo.Concentrador != null, x => new DispositivoDto
            {
                Activo = x.Dispositivo.Activo,
                Codigo = x.Dispositivo.Codigo,
                Descripcion = x.Dispositivo.Descripcion,
                ClaseDriver = x.Dispositivo.Concentrador.Codigo,
                EstadoCorrecto = x.Dispositivo.EstadoCorrecto
            }).OrderBy(x => x.ClaseDriver);

            var labels = new List<string>();
            var padres = new List<string>();
            var codigos = new List<string>();
            var activos = new List<bool>();
            var estados = new List<bool>();

            labels.Add("Dispositivos");
            padres.Add("");
            codigos.Add("Dispositivos");
            activos.Add(true);
            estados.Add(true);

            foreach (var p in dispositivos.GroupBy(x=> x.ClaseDriver))
            {
                
                labels.Add(p.Key.Split('.', ',')[3].Replace("Driver",""));
                padres.Add("Dispositivos");
                codigos.Add(p.Key.Split('.', ',')[3].Replace("Driver", ""));

                activos.Add(p.Any(x => x.Activo));
                estados.Add(p.Where(x => x.Activo).All(y => !y.EsConcentrador ? y.EstadoCorrecto : dispositivosVirtual.Where(x => x.ClaseDriver == y.Codigo && x.Activo).All(x => x.EstadoCorrecto)));

            }
            foreach (var d in dispositivos)
            {
                labels.Add(d.Descripcion);
                padres.Add(d.ClaseDriver.Split('.', ',')[3].Replace("Driver", ""));
                codigos.Add(d.Codigo);
                activos.Add(d.Activo);
                if (d.EsConcentrador)
                {
                    estados.Add(dispositivosVirtual.Where(x => x.ClaseDriver == d.Codigo && x.Activo).All(x => x.EstadoCorrecto));
                }
                else
                {
                    estados.Add(d.EstadoCorrecto);
                }
            }
            foreach (var d in dispositivosVirtual){
               
                labels.Add(d.Descripcion);
                padres.Add(d.ClaseDriver);
                codigos.Add(d.Codigo);
                activos.Add(d.Activo);
                estados.Add(d.EstadoCorrecto);
            }



            return Json(new
                {
                    Labels=labels,
                    Padres= padres,
                    Codigos = codigos,
                    Activos = activos,
                    EstadoCorrecto = estados,
                    FechaUltimoEstado = estado != null && estado.Fecha != null ? (DateTime?)estado.Fecha : null
            }, JsonRequestBehavior.AllowGet);;
        }


        public ActionResult RedireccionarPrueba(string codigoDispositivo, string path)
        {
            var dispositivo = repositorio.Obtener<Dispositivo>(x => x.Codigo == codigoDispositivo);
            if(dispositivo.Concentrador != null)
            {
                dispositivo = dispositivo.Concentrador;
                path = "itc";
            }
            return RedirectToAction("Probar", path, new { id = dispositivo.Id });
        }
    }
}
