using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Molinos.Orquest.Web.Controllers
{
    [AllowAnonymous]
    public class PruebaSensorController : BaseController
    {
        private readonly IEnumerable<string> drivers;

        public PruebaSensorController(IRepositorioFactory repositorio, IDriverFactory driverFactory, IServicioOrquestador servicio, ILogger log)
            : base(repositorio, servicio, log)
        {
            drivers = driverFactory.DriversDisponibles<IDriverSensor>();
        }

        public ActionResult consultarSensor(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                log.Info($"Consultando Estado Sensor: {codigo}");
                var resultado = servicio.Ejecutar(new EjecutarConsultaEstadoSensor { CodigoDispositivo = codigo }) as ResultadoEstadoSensor;
                jsonResult = Json(new
                {
                    Codigo = resultado.Mensaje.Codigo,
                    Mensaje = String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion),
                    CodigoDispositivoSensor = resultado?.CodigoDispositivoSensor,
                    EstadoActivo = resultado?.EstadoActivo
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult = Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }

        public ActionResult NotificarEstadoSendor(string codigo)
        {
            JsonResult jsonResult;
            try
            {
                log.Info($"Ejecutando Notificar Estado Actual del Sensor: {codigo}");
                var resultado = servicio.Ejecutar(new EjecutarNotificacionEstadoSensor { CodigoDispositivo = codigo });
                jsonResult = Json(new
                {
                    Codigo = resultado.Mensaje.Codigo,
                    Mensaje = String.Format("{0}: {1}-{2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion)
                }, JsonRequestBehavior.AllowGet);
                log.Info($"Ejecutando Notificar Estado Actual del Sensor Exitosamente.");
            }
            catch (Exception ex)
            {
                log.Error(ex, "No se pudo acceder al orquestador de dispositivos");
                jsonResult = Json(new
                {
                    Codigo = 999,
                    Mensaje = String.Format("{0}: {1}", codigo, Textos.PruebaItc_ErrorServicio)
                }, JsonRequestBehavior.AllowGet);
            }
            return jsonResult;
        }
    }
}