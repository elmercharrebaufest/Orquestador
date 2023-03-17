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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverMolineteBasic : DriverBase, IDriverMolinete
    {
        private string codigoDispositivo;
        private ConfigMolinete configuracionMolinete;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.LecturaTarjetaMolinete, CodigosEventos.NuevoTransito, CodigosEventos.LecturaQr,
            /*CodigosEventos.ErrorLecturaQr,*/ CodigosEventos.ErrorConexionDispositivo, CodigosEventos.PulsadorEmergenciaUtilizado };
        private bool notificaEventos = false;
        private string tarjeta;
        private string direccion;
        private int? fichadaId;
        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);
        private HttpClient client = new HttpClient();
        private bool estadoBotonEmergencia;


        public override Type TipoDispositivo
        {
            get { return typeof(ConfigMolinete); }
        }

        public override IEnumerable<string> EventosSoportados
        {
            get { return eventosSoportados; }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configuracionMolinete = (ConfigMolinete)configuracion;
            notificaEventos = true;
            estadoBotonEmergencia = false;
            Log.Debug($"Iniciando Driver de Molinete {codigo}");
            Task.Run(() =>
            {
                while (notificaEventos)
                {
                    try
                    {
                        ConsultarEstado();
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
                        {
                            Log.Debug($"Conexion reestablecida con el Molinete {codigoDispositivo}");
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e, "Error al ConsultarEstado del Molinete {0}", codigoDispositivo);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de Molinete= {0}", codigoDispositivo);
                            NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }
                    }
                    Thread.Sleep(configuracionMolinete.IntervaloPooling);
                }
                finCiclo.Set();
            });
        }

        public override void VerificarDispositivo()
        {
            if ((falloUltimaConexion ?? true))
            {
                throw new ConexionDispositivoDriverException("");
            }
        }
        private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
        {
            try
            {
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = codigoEvento,
                    Datos = e != null ? new Dictionary<string, string>
                            {
                                {"Error", e.Message},
                                {"Detalle", e.StackTrace}
                            } : new Dictionary<string, string>()
                };
                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Molinete {0}: No se pudo notificar el evento {1}", codigoDispositivo, codigoEvento);
            }
        }
        private void ConsultarEstado()
        {
            var respuesta = EnviarConsulta();

            if (respuesta.NuevoTransito.Direccion != "null" || respuesta.NuevoTransito.Denegado != false || respuesta.NuevoTransito.SinTransito != false)
            {
                NotificarTransito(respuesta.NuevoTransito.Direccion != "null" ? respuesta.NuevoTransito.Direccion : direccion,
                    respuesta.NuevoTransito.Denegado, respuesta.NuevoTransito.SinTransito);
                if (respuesta.NuevoTransito.Denegado != true)
                {
                    tarjeta = null;
                    direccion = null;
                }
            }
            if (respuesta.NuevaTarjeta.Tarjeta != "null" && !string.IsNullOrEmpty(respuesta.NuevaTarjeta.Tarjeta))
            {
                NotificarLecturaTarjeta(respuesta.NuevaTarjeta.Tarjeta, respuesta.NuevaTarjeta.Lector);
                tarjeta = respuesta.NuevaTarjeta.Tarjeta;
                direccion = respuesta.NuevaTarjeta.Lector;
            }
            if (respuesta.NuevoQR.QR != "null" && !string.IsNullOrEmpty(respuesta.NuevoQR.QR) && respuesta.NuevoQR.Lector != "null")
            {
                Log.Debug($"Se entro al IF de QR: {respuesta.NuevoQR.QR}");
                var lecturaQr = respuesta.NuevoQR.QR;
                var datosPersonales = new DatosPersonalesQrDto();

                try
                {
                    Log.Debug($"ingresando a ProcesarQrCovidGobierno: {datosPersonales}");
                    ProcesarQrCovidGobierno(lecturaQr, datosPersonales);
                    Log.Debug($"ingresando a ProcesarQrCovidSantaFe: {datosPersonales}");
                    ProcesarQrCovidSantaFe(lecturaQr, datosPersonales);
                    Log.Debug($"ingresando a ProcesarQrDni: {datosPersonales}");
                    ProcesarQrDni(lecturaQr, datosPersonales);
                    Log.Debug($"fin procesadores: {JsonConvert.SerializeObject(datosPersonales)}");

                    if (!datosPersonales.GetType().GetProperties().Any(prop => prop.GetValue(datosPersonales, null) == null))
                    {
                        direccion = respuesta.NuevaTarjeta.Lector;
                        NotificarLecturaQr(datosPersonales, respuesta.NuevoQR);
                    }
                    else
                    {
                        datosPersonales.TipoDeQr = TipoQr.QrInvalido;
                        throw new Exception("QR Inválido");
                    }
                }
                catch (Exception error)
                {
                    NotificarLecturaQr(datosPersonales, respuesta.NuevoQR, error.Message);
                }
            }
            if (bool.Parse(respuesta.PulsadorEmergencia.Estado) != estadoBotonEmergencia)
            {
                NotificarPulsadorEmergencia(respuesta.PulsadorEmergencia.Estado);
                estadoBotonEmergencia = !estadoBotonEmergencia;
            }
        }

        private void ProcesarQrCovidGobierno(string respuesta, DatosPersonalesQrDto datosPersonales)
        {
            Log.Debug($"adentro de ProcesarQrCovidGobierno, dato recibido : {respuesta}");
            if (respuesta.Contains("form-ddjj.argentina.gob.ar"))
            {
                var htmlPagina = "";
                var context = BrowsingContext.New(Configuration.Default);
                try
                {
                    var response = client.GetAsync(respuesta).Result;
                    htmlPagina = response.Content.ReadAsStringAsync().Result;
                }
                catch (Exception e)
                {
                    Log.Error(e, $"No se pudo obtener datos de web Covid Gobierno: {respuesta}");
                    datosPersonales.TipoDeQr = TipoQr.QrInvalido;
                    throw new Exception("Error al leer QR Covid Gobierno.");
                }

                var document = context.OpenAsync(req => req.Content(htmlPagina)).Result;

                var datos = document.GetElementsByClassName("col-sm-8");

                if (datos.Any())
                {
                    var nombreApellido = datos[1].TextContent.Split(',');
                    var dni = 0;

                    datosPersonales.Dni = Int32.TryParse(datos[0].TextContent, out dni) ? dni.ToString() : null;
                    datosPersonales.Nombre = nombreApellido[1];
                    datosPersonales.Apellido = nombreApellido[0];
                    datosPersonales.Cuit = "";
                    datosPersonales.TipoDeQr = TipoQr.CovidGobierno;
                }
                else
                {
                    datosPersonales.Dni = "-";
                    datosPersonales.Nombre = "-";
                    datosPersonales.Apellido = "-";
                    datosPersonales.Cuit = "-";
                    datosPersonales.TipoDeQr = TipoQr.CovidGobierno;

                    throw new Exception("Vencido");
                }

            }
        }
        private void ProcesarQrCovidSantaFe(string respuesta, DatosPersonalesQrDto datosPersonales)
        {

            if (ValidateJSON(respuesta))
            {
                var resp = JsonConvert.DeserializeObject<dynamic>(respuesta);
                var fechaExpiracion = (resp.expira.Value.GetType().Name == "String") ? DateTime.Parse(resp.expira.Value) : resp.expira.Value;

                datosPersonales.Dni = resp.nroDocumento;
                datosPersonales.Nombre = resp.nombre;
                datosPersonales.Apellido = resp.apellido;
                datosPersonales.Cuit = "";
                datosPersonales.TipoDeQr = TipoQr.CovidSantaFe;

                if (resp.resultado.Value != "asintomatico" || fechaExpiracion < DateTime.Now)
                {
                    throw new Exception((fechaExpiracion < DateTime.Now) ? "Vencido" : "Sintomatico");
                }
            }
        }


        private void ProcesarQrDni(string respuesta, DatosPersonalesQrDto datosPersonales)
        {
            var datosLectura = respuesta.Split('@');

            if (datosLectura.Count() == 9)
            {
                datosPersonales.Dni = datosLectura[4];
                datosPersonales.Nombre = datosLectura[2];
                datosPersonales.Apellido = datosLectura[1];
                datosPersonales.Cuit = datosLectura[8];
                datosPersonales.TipoDeQr = TipoQr.Dni;
            }
        }
        public ConsultaMolinete EnviarConsulta()
        {
            var request = new EstadoMolinete { ConsultaEstados = new ConsultaEstados { EstadoGeneral = true } };

            HttpResponseMessage response = PostAsJsonAsync(client, configuracionMolinete.DireccionUrl, request).Result;
            response.EnsureSuccessStatusCode();

            var resultado = response.Content.ReadAsStringAsync().Result;
            var respuesta = (ValidateJSON(resultado)) ? JsonConvert.DeserializeObject<ConsultaMolinete>(resultado) : generarDto(resultado);

            return respuesta;
        }

        private void NotificarLecturaTarjeta(string tarjeta, string sentido)
        {
            try
            {
                Log.Debug($"Tarjeta Leída: {tarjeta}");
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.LecturaTarjetaMolinete,
                    Datos = new Dictionary<string, string>
                    {
                        { "Tarjeta", tarjeta },
                        { "Sentido", sentido }
                    }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.LecturaTarjetaMolinete);
            }
        }
        private void NotificarTransito(string direccion, bool denegado, bool sinTransito)
        {
            try
            {
                Log.Debug($"Tarjeta Leída: {tarjeta}");
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.NuevoTransito,
                    Datos = new Dictionary<string, string>
                    {
                        { "Tarjeta", tarjeta },
                        { "Direccion", direccion },
                        { "Denegado", denegado.ToString() },
                        { "SinTransito", sinTransito.ToString() }
                    }
                };

                if (fichadaId.HasValue)
                {
                    notification.Datos.Add("FichadaManual", fichadaId.ToString());
                    fichadaId = null;
                }

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.NuevoTransito);
            }
        }

        private void NotificarLecturaQr(DatosPersonalesQrDto datosPersonales, NuevoQR qrRecibido, string error = null)
        {
            try
            {
                Log.Debug($"Qr Leído: {datosPersonales}, Tipo de Qr: {datosPersonales.TipoDeQr.ToString()} ");
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.LecturaQr,
                    Datos = new Dictionary<string, string>
                    {
                        { "Lector", qrRecibido.Lector },
                        { "QrRecibido", qrRecibido.QR },

                        { "Dni", datosPersonales.Dni },
                        { "Nombre", datosPersonales.Nombre },
                        { "Apellido", datosPersonales.Apellido },
                        { "Cuit", datosPersonales.Cuit },
                        { "TipoDeQr", datosPersonales.TipoDeQr.ToString() }
                    }
                };

                if (error != null)
                {
                    notification.Datos.Add("Error", error);
                }

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.LecturaQr);
            }
        }

        private void NotificarPulsadorEmergencia(string estado)
        {
            try
            {
                Log.Debug($"Pulsador de Emergencia Utilizado.");
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.PulsadorEmergenciaUtilizado,
                    Datos = new Dictionary<string, string>
                    {
                        { "Estado", estado },
                        { "Molinete", configuracionMolinete.Dispositivo.Descripcion }
                    }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.PulsadorEmergenciaUtilizado);
            }
        }

        public void HabilitarTransito(string direccion, string tarjeta, int? fichadaId)
        {
            this.tarjeta = tarjeta;
            this.direccion = direccion;
            this.fichadaId = fichadaId;

            var request = new HabilitacionMolinete { HabilitarTransito = new HabilitarTransito { Direccion = direccion, Timeout = configuracionMolinete.TimeoutHabilitacion } };

            HttpResponseMessage response = PostAsJsonAsync(client, configuracionMolinete.DireccionUrl, request).Result;
            response.EnsureSuccessStatusCode();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notificaEventos = false;
                finCiclo.WaitOne();
                finCiclo.Dispose();
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

        public async Task<HttpResponseMessage> PostAsJsonAsync<TModel>(HttpClient client, string requestUrl, TModel model)
        {
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(model);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            return await client.PostAsync(requestUrl, stringContent);
        }

        public ConsultaMolinete generarDto(string mijson)
        {
            string restoDelJson = "";
            string qr = "";

            try
            {
                //json covid SantaFe

                int startIndex = mijson.IndexOf('{', mijson.IndexOf('{', 1) + 1);
                int endIndex = mijson.IndexOf('}') + 1;


                string valor1 = mijson.Substring(0, startIndex);
                string valor3 = mijson.Substring(endIndex);

                restoDelJson = valor1 + valor3;
                qr = mijson.Substring(startIndex, endIndex - startIndex);
            }
            catch (Exception e)
            {
                //dni con Comillas

                int startIndex = mijson.IndexOf(':', mijson.IndexOf(':', 1) + 1) + 1;
                int endIndex = mijson.IndexOf(',');

                string valor1 = mijson.Substring(0, startIndex) + "\"";
                string valor3 = "\"" + mijson.Substring(endIndex);

                restoDelJson = valor1 + valor3;
                qr = mijson.Substring(startIndex, endIndex - startIndex).Replace('\"', '@');
                qr = qr.Substring(2, qr.Length - 3).Trim();
            }

            var jObject = JsonConvert.DeserializeObject<ConsultaMolinete>(restoDelJson);
            jObject.NuevoQR.QR = qr;

            return jObject;
        }
    }
}
