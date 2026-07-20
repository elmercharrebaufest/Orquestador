using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Enums;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverLectorPatente : DriverBase, IDriverSensor, IDriverLogico
    {
        private IDriverItc driverItc;
        private IDriverConDireccionIp driverConIp;
        private string codigoDispositivo;
        private string entrada;
        private ConfigSensor configSensor;
        private string endpointPath;
        private int timeoutMs;

        private readonly List<string> eventosSoportados = new List<string>
        {
            CodigosEventos.EntradaActivada,
        };

        public override IEnumerable<string> EventosSoportados => eventosSoportados;

        public override Type TipoDispositivo => typeof(ConfigSensor);

        public IDriver DriverFisico
        {
            set
            {
                driverConIp = (IDriverConDireccionIp)value;
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configSensor = (ConfigSensor)configuracion;
            entrada = configSensor.NumeroEntrada.ToString(CultureInfo.InvariantCulture);
            endpointPath = ConfigurationManager.AppSettings["DriverLectorPatente.EndpointPath"];
            timeoutMs = int.TryParse(ConfigurationManager.AppSettings["DriverLectorPatente.TimeoutMs"], out var t) ? t : 5000;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }

        public ResultadoEstadoSensor ConsultaEstadoActual()
        {
            return new ResultadoEstadoSensor
            {
                CodigoDispositivoSensor = codigoDispositivo,
                EstadoActivo = driverItc.ConsultarEstadoActual(configSensor.NumeroEntrada),
                Mensaje = Mensaje.ResultadoOK()
            };
        }

        public void NotificarEstadoActualSensor()
        {
            driverItc.NotificarEstadoActual(configSensor.NumeroEntrada);
        }

        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {
            var notificacion = evento.Notificacion;
            if (!EsEventoParaDispositivo(notificacion))
                return;

            ProcesarEventoEntrada(notificacion);
        }

        private void ProcesarEventoEntrada(NotificacionEvento notificacion)
        {
            var datos = new Dictionary<string, string>(notificacion.Datos ?? new Dictionary<string, string>());
            var detalleCamaraCIV = new List<DetalleCamaraCIV>();

            try
            {
                var resultados = ConsultarEndpoint();
                foreach (var foto in resultados)
                {
                    var patente = foto.Datos?.Patente == "unknown" ? string.Empty : foto.Datos?.Patente;
                    var detalle = new DetalleCamaraCIV
                    {
                        CodigoCamara = codigoDispositivo,
                        ProveedorALPR = ProveedorALPR.HikVision.ToString(),
                        Intentos = 1,
                        Patente = patente,
                        RutaImagen = foto.Datos?.Url,
                        Certeza = foto.Datos?.Confianza,
                    };
                    detalleCamaraCIV.Add(detalle);
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "[DriverLectorPatente:{0}] Error al consultar endpoint. Se continúa sin patente.", codigoDispositivo);
                var detalleConError = new DetalleCamaraCIV
                {
                    CodigoCamara = codigoDispositivo,
                    ProveedorALPR = ProveedorALPR.HikVision.ToString(),
                    Intentos = 1,
                    Patente = string.Empty,
                    RutaImagen = string.Empty,
                    Certeza = 0,
                    Error = ex.Message,
                };
                detalleCamaraCIV.Add(detalleConError);
            }

            if (detalleCamaraCIV.Count > 0)
                datos[DatosNotificacion.Detalle] = detalleCamaraCIV.ToJson();

            var eventoNotification = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.EntradaActivada,
                    Datos = datos
                }
            };
            Log.Debug("[DriverLectorPatente:{0}]", eventoNotification.ToJson());
            OnEventoDriver(eventoNotification);
        }

        private IList<ResultadoEndpointDto> ConsultarEndpoint()
        {
            var url = $"http://{driverConIp.DireccionIp}{endpointPath}";
            Log.Debug("[DriverLectorPatente:{0}] POST {1}", codigoDispositivo, url);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = timeoutMs;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = 0;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                Log.Debug("[DriverLectorPatente:{0}] Respuesta: {1}", codigoDispositivo, json);
                return JsonConvert.DeserializeObject<List<ResultadoEndpointDto>>(json);
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null
                    || !notificacion.Datos.ContainsKey("Entrada")
                    || notificacion.Datos["Entrada"] == entrada);
        }

        private class ResultadoEndpointDto
        {
            [JsonProperty("datos")]
            public DatosEndpointDto Datos { get; set; }
        }

        private class DatosEndpointDto
        {
            [JsonProperty("url_foto")]
            public string Url { get; set; }

            [JsonProperty("patente")]
            public string Patente { get; set; }

            [JsonProperty("confianza")]
            public float Confianza { get; set; }
        }
    }
}
