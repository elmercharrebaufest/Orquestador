using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBalanzaPuerto : DriverBase, IDriverBalanzaPuerto
    {
        private readonly object lockComando = new object();

        private bool notificaEventos = false;
        public string codigoBalanzaPuerto;
        private ConfigBalanzaPuerto configBalanzaPuerto;

        private ITcpCommandClient cliente;
        private bool enableAsyncProcess = true;

        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.BalanzadaRecibida, CodigosEventos.ConexionDispositivoCorrecta, CodigosEventos.ErrorConexionDispositivo };
        public Dictionary<string, string> PalabrasReservadas = new Dictionary<string, string>() 
        {
                { "STARCOM", "inicio" },
                { "TST1", "inicio" },
                { "BATRM", "balanzada" },
                //{ "BATR", "balanzada" },
                //{ "TST2", "balanzada" },
                { "STORCOM", "fin" },
                { "TST3", "fin" },
                { "START:", "fechaInicio" },
                { "M:", "commodity" },
                { "BOD.:", "bodega" },
                { "EXP.:", "exportador" },
                { "DES.:", "destino" },
                { "BUQ.:", "vapor" },
                { "TNW:", "pesoProgramado" },
                { "TAW:", "toneladasaw" },
                { "G:", "pesoBruto" },
                { "MT:", "pesoTara" },
                { "T:", "pesoTara" },
                { "CAP:", "capacidad" }
            };

        public override IEnumerable<string> EventosSoportados
        {
            get { return eventosSoportados; }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigBalanzaPuerto); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoBalanzaPuerto = codigo;
            configBalanzaPuerto = (ConfigBalanzaPuerto)configuracion;

            notificaEventos = true;
            if (cliente == null)
            {
                bool.TryParse(ConfigurationManager.AppSettings["DriverBalanzaPuerto.LoguearTCP"], out bool logTcp);
                cliente = new TcpCommandClient(
                    configBalanzaPuerto.DireccionIp,
                    configBalanzaPuerto.Puerto,
                    configBalanzaPuerto.LongFrase,
                    configBalanzaPuerto.TimeoutLectura,
                    Log,
                    false,
                    logTcp);
            }

            Log.Debug("Iniciando Driver de Balanza Puerto {0}", codigo);
            Regex soloNumerosBalanza = new Regex(@"[^-?\d]");
            if (!PalabrasReservadas.ContainsKey(codigo))
            {
                PalabrasReservadas.Add(codigoBalanzaPuerto + "TST2", "balanzada");
                PalabrasReservadas.Add(codigoBalanzaPuerto + "BATR", "balanzada");
                PalabrasReservadas.Add(codigo, soloNumerosBalanza.Replace(codigo, ""));
            }

            if (enableAsyncProcess)
            {
                Task.Run(() =>
                {
                    while (notificaEventos)
                    {
                        try
                        {
                            var retorno = ConsultaBalanzada(null);
                            if (retorno.Count != 0)
                            {
                                NotificarBalanzada(retorno);
                            }
                            //Cuando no hay estado anterior se lanza el evento
                            if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
                            {
                                Log.Debug("Nueva Conexión a Balanza={0}", codigoBalanzaPuerto);
                                NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                                falloUltimaConexion = false;
                                errorUltimaConexion = null;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e, "Error al ConsultarBalanzada de la balanza {0}", codigoBalanzaPuerto);
                            //Cuando no hay estado anterior se lanza el evento
                            if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                            {
                                Log.Debug("Desconexión de la balanza={0}", codigoBalanzaPuerto);
                                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                                falloUltimaConexion = true;
                                errorUltimaConexion = e;
                            }
                        }

                        Thread.Sleep(configBalanzaPuerto.IntervaloPolling);
                    }
                    finCiclo.Set();
                });
            }
        }

        private void NotificarBalanzada(Dictionary<string, string> balanzada)
        {
            try
            {
                Log.Debug("Balanzada para la Balanza={0} ", codigoBalanzaPuerto);
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoBalanzaPuerto,
                    CodigoEvento = CodigosEventos.BalanzadaRecibida,
                    Datos = balanzada
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.LecturaTarjetaRecibida);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notificaEventos = false;
                finCiclo.WaitOne();
                finCiclo.Dispose();
                cliente.Dispose();
            }
        }

        private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
        {
            try
            {
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoBalanzaPuerto,
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
                Log.Error(ex, "PLC {0}: No se pudo notificar el evento {1}", codigoBalanzaPuerto, codigoEvento);
            }
        }

        private string EnviarComando(string comando)
        {
            string frase;
            try
            {
                lock (lockComando)
                {
                    if (!cliente.Conectado)
                        cliente.ReConectar();

                    frase = cliente.EnviarComando(comando, 0, configBalanzaPuerto.LongFrase);
                }
            }
            catch (Exception e)
            {
                throw new DriverException("Error al Conectar con el dispositivo", e);
            }

            return frase;
        }

        public override void VerificarDispositivo()
        {
            if (cliente == null || !cliente.Conectado)
            {
                throw new ConexionDispositivoDriverException("");
            }
        }

        public Dictionary<string, string> ConsultaBalanzada(int? IdBalanzada)
        {
            return ConsultaBalanzada(IdBalanzada, 0);
        }

        private Dictionary<string, string> ConsultaBalanzada(int? IdBalanzada, int intento)
        {
            Dictionary<string, string> resultado;
            var respuesta = string.Empty;

            try
            {
                var stringIdBalanzada = 
                    (IdBalanzada == null) ? 
                        string.Empty : 
                        "," + IdBalanzada.ToString().PadLeft(configBalanzaPuerto.CantidadCaracteresTotal, configBalanzaPuerto.CaracterIzquierdaACompletar.First());

                if (IdBalanzada.HasValue)
                    respuesta = GetObjectFromCache(IdBalanzada.Value + configBalanzaPuerto.Dispositivo.Codigo, Log);

                if (string.IsNullOrEmpty(respuesta))
                {
                    respuesta = EnviarComando(configBalanzaPuerto.ComandoConsulta + stringIdBalanzada);
                    
                    if(respuesta != null)
                        respuesta = respuesta.TrimEnd('\0');
                }                

                if (!string.IsNullOrEmpty(respuesta))
                {
                    if (respuesta != "NULL" || IdBalanzada.HasValue)
                    {
                        Log.Info("Comando P," + IdBalanzada + " || Resultado: " + respuesta);

                        try
                        {
                            if (!IdBalanzada.HasValue || ObtenerId(respuesta) == IdBalanzada.Value)
                            {
                                Log.Debug("Comando P," + IdBalanzada + " || voy a convertir");

                                if (respuesta.Split(';').Length >= 5) // Los nuevos strings tienen al menos 5 partes
                                    resultado = I410ABSParser.ConvertirADictionary(respuesta);
                                else
                                    resultado = ConvertirADictionary(respuesta, IdBalanzada);

                                Log.Debug("Comando P," + IdBalanzada + " || Resultado: " + resultado.Count + "voy a cachear");

                                SetObjectToCache(ObtenerId(respuesta) + configBalanzaPuerto.Dispositivo.Codigo, 24, respuesta, Log);
                                Log.Debug("Comando P," + IdBalanzada + " || salgo");
                            }
                            else
                            {
                                Log.Warn("Error al intentar convertir la respuesta de la balanza {0} con IdBalanzada {1}. Se reintentará la operación.", codigoBalanzaPuerto, IdBalanzada);
                                resultado = Reintentar(++intento, respuesta, IdBalanzada, false);
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, "Error al intentar convertir la respuesta de la balanza {0} con IdBalanzada {1}. Se reintentará la operación.", codigoBalanzaPuerto, IdBalanzada);
                            resultado = Reintentar(++intento, respuesta, IdBalanzada, false);
                        }
                    }
                    else
                        resultado = new Dictionary<string, string>();
                }
                else
                {
                    Log.Error(
                        "Error al intentar obtener la respuesta de la balanza {0} con IdBalanzada {1} por ser nula o vacía. " +
                        "Se reintentará la operación. ", codigoBalanzaPuerto, IdBalanzada);

                    resultado = this.Reintentar(++intento, respuesta, IdBalanzada, true);
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoBalanzaPuerto), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoBalanzaPuerto), e);
            }
            catch (FormatException e)
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para el comando {1}", codigoBalanzaPuerto, configBalanzaPuerto.ComandoConsulta), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoBalanzaPuerto), e);
            }

            return resultado;
        }

        private Dictionary<string, string> Reintentar(int intento, string respuesta, int? IdBalanzada, bool reconectar = false)
        {
            Dictionary<string, string> respuestaAsDictionary = null;

            if (intento <= 5)
            {
                Log.Info("Reintento " + intento + "- " + IdBalanzada);

                // Se intenta reconectar el cliente luego de 3 intentos fallidos, por si el objeto cliente usado quedó inconsistente.
                if (reconectar && intento > 3)
                    cliente.ReConectarV2();
                                
                Thread.Sleep(configBalanzaPuerto.IntervaloPolling + configBalanzaPuerto.IntervaloPolling * 1 / 3);

                respuestaAsDictionary = 
                    (!string.IsNullOrEmpty(respuesta)) ? 
                        ConsultaBalanzada(IdBalanzada ?? ObtenerId(respuesta), intento) : 
                        ConsultaBalanzada(IdBalanzada, intento);
            }
            else
            {
                string mensaje = "No se pudo obtener una respuesta válida después de 5 intentos. Respuesta: " + respuesta;
                Log.Warn(mensaje);
                throw new FormatException(respuesta ?? mensaje);
            }

            return respuestaAsDictionary;
        }

        private string BalanzadaNombresDescriptivos(string key)
        {
            string value;
            if (!PalabrasReservadas.TryGetValue(key, out value))
            {
                PalabrasReservadas.TryGetValue(codigoBalanzaPuerto + key, out value);
            }
            return value;
        }

        private int ObtenerId(string consulta)
        {
            var id = consulta.Split(';').GetValue(0).ToString();
            
            if (id.Length == configBalanzaPuerto.CantidadCaracteresTotal)
            {
                return Convert.ToInt32(id);
            }
            else
            {
                throw new FormatException(consulta);
            }
        }

        public Dictionary<string, string> ConvertirADictionary(string respuesta, int? IdBalanzada)
        {
            if (respuesta.Contains(codigoBalanzaPuerto + " ERR"))
            {
                var respuestaAsDictionary = new Dictionary<string, string>();

                if (respuesta.Contains("MSC 41:UPD FACILITY ACTIVE"))
                    respuestaAsDictionary.Add("tipoBalanzada", "error41");
                else if (respuesta.Contains("MSC 44:EMST ACTIVE"))
                    respuestaAsDictionary.Add("tipoBalanzada", "error44");
                else
                    respuestaAsDictionary.Add("tipoBalanzada", "error");

                respuestaAsDictionary.Add("numeroBalanza", BalanzadaNombresDescriptivos(codigoBalanzaPuerto));
                var idConFechaError = respuesta.Split(' ').GetValue(0).ToString() + respuesta.Split(' ').GetValue(1).ToString();
                respuestaAsDictionary.Add("id", idConFechaError.Split(';').GetValue(0).ToString());
                respuestaAsDictionary.Add("fecha", idConFechaError.Split(';').GetValue(1).ToString().Trim().Replace(" ", ""));
                return respuestaAsDictionary;
            }
            else if (respuesta.Contains(codigoBalanzaPuerto + "BATR") || respuesta.Contains(codigoBalanzaPuerto + "TST2"))
            {
                respuesta = respuesta.Substring(0, respuesta.IndexOf('h') + 1);
            }
            else
            {
                respuesta = respuesta.Substring(0, respuesta.LastIndexOf('g') + 1);
            }

            var keys = Regex.Matches(respuesta, @"(" + codigoBalanzaPuerto + "|TST1|STARCOM|TST2|BATRM|BATR|TST3|STORCOM|M:|START:|BOD.:|EXP.:|DES.:|BUQ.:|TNW:|TAW:|G:|MT:|T:|CAP:)")
                .Cast<Match>()
                .Select(m => BalanzadaNombresDescriptivos(m.Value))
                .ToList();

            var values = respuesta.Split(PalabrasReservadas.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x != "M")
                .Select(s => s.Trim())
                .ToList();

            keys.Add("numeroBalanza");
            values.Add(keys.First());
            keys.RemoveAt(0);
            keys.Add("tipoBalanzada");
            var tipo = keys.First();
            if (tipo != "balanzada")
            {
                keys.RemoveAll(x => x == "balanzada");
            }
            values.Add(keys.First());
            keys.RemoveAt(0);
            var idConFecha = values.First().Split(';');
            keys.Add("id");
            var esNumero = Int32.TryParse(idConFecha.GetValue(0).ToString(), out int r);
            values.Add(esNumero ? idConFecha.GetValue(0).ToString() : idConFecha.GetValue(0).ToString().Substring(1));
            keys.Add("fecha");

            var fechaPLC = idConFecha.GetValue(1).ToString().Trim().Insert(8, DateTime.Now.ToString("yy"));
            values.Add(fechaPLC);

            values.RemoveAt(0);

            var pares = keys.Zip(values, (k, v) => new { k, v }).ToDictionary(val => val.k, val => val.v);

            Regex soloNumeros = new Regex(@"[^-?\d]");
            pares["toneladasaw"] = soloNumeros.Replace(pares["toneladasaw"], "");
            if (pares["tipoBalanzada"] == "balanzada")
            {
                pares["pesoBruto"] = soloNumeros.Replace(pares["pesoBruto"], "");
                pares["pesoTara"] = soloNumeros.Replace(pares["pesoTara"], "");
                var pesoNeto = long.Parse(pares["pesoBruto"]) - long.Parse(pares["pesoTara"]);
                pares.Add("pesoNeto", pesoNeto.ToString());
            }
            else if (pares["tipoBalanzada"] != "error")
            {
                pares["pesoProgramado"] = soloNumeros.Replace(pares["pesoProgramado"], "");
            }

            return pares;
        }

        /// <summary>
        /// Borra la balanzada especificada.
        /// </summary>
        /// <param name="IdBalanzada"></param>
        /// <returns>Un diccionario con clave idbalanzada y valor vacio si no borró nada y un diccionario con clave y valor idbalanzada si borró.</returns>
        public bool BorrarBalanzada(int IdBalanzada)
        {
            try
            {
                string balanzadaABorrar = IdBalanzada.ToString().PadLeft(configBalanzaPuerto.CantidadCaracteresTotal, configBalanzaPuerto.CaracterIzquierdaACompletar[0]);
                //var borrado = EnviarComando(configBalanzaPuerto.ComandoBorrado + "," + balanzadaABorrar);
                //Log.Info("Comando D," + IdBalanzada + " || Resultado: " + borrado.TrimEnd());

                //return borrado != null;
                return true;
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoBalanzaPuerto), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoBalanzaPuerto), e);
            }
            catch (FormatException e)
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para el comando {1}", codigoBalanzaPuerto, configBalanzaPuerto.ComandoBorrado), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoBalanzaPuerto), e);
            }
        }

        public Dictionary<string, bool> BorrarBalanzadasPorRango(int IdBalanzadaInicio, int IdBalanzadaFin)
        {

            var diccionarioRetorno = new Dictionary<string, bool>();
            for (int i = IdBalanzadaInicio; i <= IdBalanzadaFin; i++)
            {
                try
                {
                    diccionarioRetorno.Add(i.ToString(), BorrarBalanzada(i));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error al borrar balanzada: " + ex.Message);
                    diccionarioRetorno.Add(i.ToString(), false);
                }
            }
            return diccionarioRetorno;

        }

        public override bool MantenerConectado()
        {
            return true;
        }

        public override void InformarEstado()
        {
            Log.Debug("Informando estado balanza {0}", codigoBalanzaPuerto);
            if (falloUltimaConexion.HasValue && falloUltimaConexion.Value)
            {
                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, errorUltimaConexion ?? new Exception(CodigosEventos.ErrorConexionDispositivo));
            }
            else
            {
                NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
            }
        }

        private static string GetObjectFromCache(string cacheItemName, ILogger Log)
        {
            ObjectCache cache = MemoryCache.Default;
            var resultado = (string)cache[cacheItemName];
            Log.Info($"Uso Cache: ({cacheItemName}) -> {resultado}");
            return resultado;
        }

        private static void SetObjectToCache(string cacheItemName, int cacheTimeInHours, string obj, ILogger Log)
        {
            ObjectCache cache = MemoryCache.Default;
            var cachedObject = (string)cache[cacheItemName];
            Log.Info($"Cacheo??: ({cacheItemName}) -> {cachedObject}");
            if (string.IsNullOrEmpty(cachedObject))
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddHours(cacheTimeInHours)
                };
                Log.Info($"Cacheo: ({cacheItemName}) -> {obj}");
                cache.Set(cacheItemName, obj, policy);
            }
        }

        public static class I410ABSParser
        {
            public static Dictionary<string, string> ConvertirADictionary(string consulta)
            {
                var partesList = consulta.Split(';').ToList();
                var partesTipoYBalanza = partesList[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var tipo = partesTipoYBalanza[0];
                var balanza = string.Join(" ", partesTipoYBalanza.Skip(1));
                balanza = SoloNumeros(balanza);
                partesList.Insert(2, balanza);
                var partes = partesList.ToArray();

                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "BalanzadaPuerto.json");
                var configJson = File.ReadAllText(configPath);
                var configRoot = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(configJson);

                switch (tipo)
                {
                    case "1":
                        return ConvertirInicio(partes, configRoot);
                    case "2":
                        return ConvertirFin(partes, configRoot);
                    case "3":
                        return ConvertirBalanzada(partes, configRoot);
                    case "4":
                        return ConvertirError(partes, configRoot);
                    default:
                        throw new Exception("No se reonoce el tipo de balanzada: " + tipo);
                }
            }

            // TODO: Ajustar este método según la forma en la que recibimos la fecha y hora. El formato asumido de fecha es ddMMyyy y de hora es HH:mm:ss.
            // El formato de salida debe ser siempre dd-MM-yyyyHH:mm
            private static string FormatearFechaHora(string fecha, string hora)
            {
                var formatoHora = hora.Length == 8 ? "HH:mm:ss" : "HH:mm";
                DateTime fechaHora = DateTime.ParseExact(fecha.Trim() + hora.Trim(), "dd/MM/yy" + formatoHora, CultureInfo.InvariantCulture);
                return fechaHora.ToString("dd-MM-yyyyHH:mm");
            }

            private static string SoloNumeros(string valor)
            {
                return new Regex(@"[^-?\d]").Replace(valor, "");
            }

            private static Dictionary<string, string> ConversionComunInicial(string[] partes, string tipoBalanzada, Dictionary<string, Dictionary<string, int>> configRoot)
            {
                var config = configRoot["comun"];
                var fecha = FormatearFechaHora(partes[config["fecha"]], partes[config["hora"]]);

                return new Dictionary<string, string>
                {
                    { "id", partes[config["id"]].Trim() },
                    { "tipoBalanzada", tipoBalanzada },
                    { "numeroBalanza", partes[config["numeroBalanza"]].Trim() },
                    { "fecha", fecha }
                };
            }

            private static Dictionary<string, string> ConvertirInicio(string[] partes, Dictionary<string, Dictionary<string, int>> configRoot)
            {
                var res = ConversionComunInicial(partes, "inicio", configRoot);
                var config = configRoot["inicio"];

                res["bodega"] = partes[config["bodega"]].Trim();
                res["vapor"] = partes[config["vapor"]].Trim();
                res["destino"] = partes[config["destino"]].Trim();
                res["exportador"] = partes[config["exportador"]].Trim();
                res["pesoProgramado"] = SoloNumeros(partes[config["pesoProgramado"]]);
                res["toneladasaw"] = SoloNumeros(partes[config["toneladasaw"]]);
                res["commodity"] = partes[config["commodity"]].Trim();

                return res;
            }

            private static Dictionary<string, string> ConvertirFin(string[] partes, Dictionary<string, Dictionary<string, int>> configRoot)
            {
                var res = ConversionComunInicial(partes, "fin", configRoot);
                var config = configRoot["fin"];

                // fechaInicio no se está utilizando actualmente en logística.
                res["fechaInicio"] = FormatearFechaHora(partes[config["fechaInicio"]], partes[config["horaInicio"]]);
                res["bodega"] = partes[config["bodega"]].Trim();
                res["vapor"] = partes[config["vapor"]].Trim();
                res["destino"] = partes[config["destino"]].Trim();
                res["exportador"] = partes[config["exportador"]].Trim();
                res["commodity"] = partes[config["commodity"]].Trim();
                res["pesoProgramado"] = SoloNumeros(partes[config["pesoProgramado"]]);
                res["toneladasaw"] = SoloNumeros(partes[config["toneladasaw"]]);

                return res;
            }

            private static Dictionary<string, string> ConvertirBalanzada(string[] partes, Dictionary<string, Dictionary<string, int>> configRoot)
            {
                var res = ConversionComunInicial(partes, "balanzada", configRoot);
                var config = configRoot["balanzada"];

                var pesoBruto = SoloNumeros(partes[config["pesoBruto"]]);
                var pesoTara = SoloNumeros(partes[config["pesoTara"]]);
                var pesoNeto = long.Parse(pesoBruto) - long.Parse(pesoTara);

                res["pesoBruto"] = pesoBruto;
                res["pesoTara"] = pesoTara;
                res["pesoNeto"] = pesoNeto.ToString();
                res["capacidad"] = partes[config["capacidad"]].Trim();
                res["toneladasaw"] = SoloNumeros(partes[config["toneladasaw"]]);

                return res;
            }

            private static Dictionary<string, string> ConvertirError(string[] partes, Dictionary<string, Dictionary<string, int>> configRoot)
            {
                // TODO: Definir los demás tipos de errores (ej error41, error44, etc.)
                var res = ConversionComunInicial(partes, "error", configRoot);
                return res;
            }
        }

        // Expose the cliente field as a public property for testing purposes
        public ITcpCommandClient Cliente
        {
            get { return this.cliente; }
            set { this.cliente = value; }
        }

        public void SetAsyncProcessEnabled(bool isEnabled)
        {
            enableAsyncProcess = isEnabled;
        }
    }
}
