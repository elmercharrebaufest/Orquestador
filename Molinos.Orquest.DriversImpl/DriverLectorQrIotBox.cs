using AngleSharp;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Enums;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverLectorQrIotBox : DriverBase, IDriverLectorQr, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigLectorQr configLector;
        private IDriverItc driverItc;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.LecturaQr, CodigosEventos.ErrorConexionDispositivo, CodigosEventos.ConexionDispositivoCorrecta };

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigLectorQr); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configLector = (ConfigLectorQr)configuracion;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {
            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {//si el evento que recibo es de señal activada entonces podria ser una lectuara o un qr error, 
                var datos = ConvertirDatos(notificacion.Datos, notificacion.CodigoEvento, out string eventoStr);
                var nuevoEvento = new EventoDriverEventArgs
                {
                    Notificacion = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = eventoStr,
                        Datos = datos
                    }
                };
                OnEventoDriver(nuevoEvento);
            }
        }

        private Dictionary<string, string> ConvertirDatos(Dictionary<string, string> datos, string eventoOriginal, out string evento)
        {
            var resultado = new Dictionary<string, string>();
            var lector = string.Empty;
            evento = eventoOriginal;
            if (datos.ContainsKey("Entrada"))
            {
                lector = datos["Entrada"];
                resultado.Add("Lector", datos["Entrada"]);
            }

            if (datos.ContainsKey("Dato"))
            {
                var lecturaQr = datos["Dato"];
                resultado.Add("QR", lecturaQr);
                try
                {
                    Log.Debug(lecturaQr);
                    ProcesarQrCovidGobierno(lecturaQr, resultado);
                    ProcesarQrCovidSantaFe(lecturaQr, resultado);
                    ProcesarQrDni(lecturaQr, resultado);
                    ProcesarQrTransitoTemporal(lecturaQr, resultado);

                    if (!resultado.ContainsKey("TipoDeQr"))
                    {
                        throw new Exception("QR Inválido.");
                    }
                    evento = CodigosEventos.LecturaQr;
                }
                catch (Exception error)
                {
                    resultado.Add("Error", error.Message);
                    evento = CodigosEventos.LecturaQr;
                }
            }

            foreach (var key in datos.Keys)
            {
                if (key != "Dato" && key != "Entrada")
                {
                    resultado.Add(key, datos[key]);
                }
            }
            return resultado;
        }

        private void ProcesarQrCovidGobierno(string respuesta, Dictionary<string, string> datosPersonales)
        {
            if (respuesta.Contains("form-ddjj.argentina.gob.ar"))
            {
                var htmlPagina = "";
                var context = BrowsingContext.New(Configuration.Default);
                datosPersonales.Add("Dni", "-");
                datosPersonales.Add("Nombre", "-");
                datosPersonales.Add("Apellido", "-");
                datosPersonales.Add("Cuit", "");
                datosPersonales.Add("TipoDeQr", TipoQr.CovidGobierno.ToString());
                try
                {
                    var response = new HttpClient().GetAsync(respuesta).Result;
                    htmlPagina = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception e)
                {
                    Log.Error(e, $"No se pudo obtener datos de web Covid Gobierno: {respuesta}");
                    datosPersonales.Add("TipoDeQr", TipoQr.QrInvalido.ToString());
                    throw new Exception("Error al leer QR Covid Gobierno.");
                }

                var document = context.OpenAsync(req => req.Content(htmlPagina)).Result;

                var datos = document.GetElementsByClassName("col-sm-8");

                if (datos.Any())
                {
                    var nombreApellido = datos[1].TextContent.Split(',');
                    var dni = 0;
                    datosPersonales["Dni"] = Int32.TryParse(datos[0].TextContent, out dni) ? dni.ToString() : null;
                    datosPersonales["Nombre"] = nombreApellido[1];
                    datosPersonales["Apellido"] = nombreApellido[0];
                    datosPersonales["Cuit"] = "";
                }
                else
                {
                    throw new Exception("Vencido");
                }
            }
        }
        private void ProcesarQrCovidSantaFe(string respuesta, Dictionary<string, string> datosPersonales)
        {

            if (ValidateJSON(respuesta) && respuesta.Contains("nroDocumento") && respuesta.Contains("resultado")) // para saber que es un QR de CovidSantaFe
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(respuesta);
                var fechaExpiracion = (resp.expira.Value.GetType().Name == "String") ? DateTime.Parse(resp.expira.Value) : resp.expira.Value;

                datosPersonales.Add("Dni", resp.nroDocumento.ToString());
                datosPersonales.Add("Nombre", resp.nombre.ToString());
                datosPersonales.Add("Apellido", resp.apellido.ToString());
                datosPersonales.Add("Cuit", "-");
                datosPersonales.Add("TipoDeQr", TipoQr.CovidSantaFe.ToString());


                if (resp.resultado.Value != "asintomatico" || fechaExpiracion < DateTime.Now)
                {
                    throw new Exception((fechaExpiracion < DateTime.Now) ? "Vencido" : "Sintomatico");
                }
            }
        }
        private void ProcesarQrDni(string respuesta, Dictionary<string, string> datosPersonales)
        {
            var datosLectura = respuesta.Split('"').Count() == 9 ? respuesta.Split('"') : respuesta.Split('@');

            if (datosLectura.Count() == 9)
            {
                datosPersonales.Add("Dni", datosLectura[4]);
                datosPersonales.Add("Nombre", datosLectura[2]);
                datosPersonales.Add("Apellido", datosLectura[1]);
                datosPersonales.Add("Cuit", datosLectura[8]);
                datosPersonales.Add("TipoDeQr", TipoQr.Dni.ToString());
            }
        }

        private void ProcesarQrTransitoTemporal(string respuesta, Dictionary<string, string> datosPersonales)
        {
            if (ValidateJSON(respuesta) && respuesta.Contains("Token")) // para saber que es un QR de TransitoTemporal
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(respuesta);

                datosPersonales.Add("Token", Convert.ToString(resp.Token));
                datosPersonales.Add("TipoDeQr", TipoQr.TransitoTemporal.ToString());
            }
        }

        private bool ValidateJSON(string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                return false;
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return (eventosSoportados.Contains(notificacion.CodigoEvento) || notificacion.CodigoEvento == CodigosEventos.EntradaActivada) &&
                   (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                    || notificacion.Datos["Entrada"] == configLector.Lector);
        }
    }
}
