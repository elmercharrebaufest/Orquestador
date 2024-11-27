using System;
using System.Collections.Generic;
using System.Globalization;
using AngleSharp.Io;
//using Molinos.ControlDeAcceso.Dominio.DTOs;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverJsonFromIotBox : DriverBase, IDriverJsonFromIotBox, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigJsonFromIotBox configDriver;
        private IDriverItc driverItc;
        private string entrada;
        private readonly ILogger log;
        private readonly List<string> eventosSoportados = new List<string> {CodigosEventos.EntradaActivada,
            CodigosEventos.ErrorConexionDispositivo, CodigosEventos.ConexionDispositivoCorrecta, CodigosEventos.TransitoOffline};

        public DriverJsonFromIotBox(ILogger log)
        {
            this.log = log;
        }

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigJsonFromIotBox); }
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configDriver = (ConfigJsonFromIotBox)configuracion;
            entrada = configDriver.NumeroEntrada.ToString(CultureInfo.InvariantCulture);
            log.Info("Codigo dispositivo: " +
                codigoDispositivo + " Entrada: " +
                entrada);
        }
        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }
        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {
            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {
                if (ExtensionesSerializacion.IsValidJson(notificacion.Datos["Dato"].ToString()))
                {
                    var nuevoEvento = DeterminarEvento(notificacion);
                    if (nuevoEvento != null) OnEventoDriver(nuevoEvento);
                    log.Info("EsEventoParaDispositivo - Dato: " +
                        notificacion.Datos["Dato"].ToString() + "Codigo evento: "
                        + notificacion.CodigoEvento);
                }
                else
                    log.Info("Evento enviado desde el dispositivo " + codigoDispositivo + " con un json inválido. (" + notificacion.Datos["Dato"].ToString() + ")");
            }
        }

        private EventoDriverEventArgs DeterminarEvento(NotificacionEvento notificacion)
        {
            var dato = JObject.Parse(notificacion.Datos["Dato"]);
            if (((JProperty)dato.First).Name == CodigosEventos.TransitoOffline)
            {
                return CrearEventoTransitoOffline(notificacion);
            }
            log.Info("Evento no soportado por el driver DriverJsonFromIotBox");
            return null;
        }

        private EventoDriverEventArgs CrearEventoTransitoOffline(NotificacionEvento notificacion)
        {

                return new EventoDriverEventArgs
                {
                    Notificacion = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = CodigosEventos.TransitoOffline,
                        Datos = notificacion.Datos,
                    }
                };
            return null;
        }

        public bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }
    }
}
