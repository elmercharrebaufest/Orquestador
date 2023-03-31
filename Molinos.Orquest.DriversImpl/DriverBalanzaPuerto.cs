using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverBalanzaPuerto : DriverBase, IDriverBalanzaPuerto
    {
        private readonly object lockComando = new object();

        private bool notificaEventos = false;
        public string codigoBalanzaPuerto;
        private ConfigBalanzaPuerto configBalanzaPuerto;

        TcpCommandClient cliente;

        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.BalanzadaRecibida, CodigosEventos.ConexionDispositivoCorrecta, CodigosEventos.ErrorConexionDispositivo };
        public Dictionary<string, string> PalabrasReservadas = new Dictionary<string, string>() {
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
            cliente = new TcpCommandClient(configBalanzaPuerto.DireccionIp, configBalanzaPuerto.Puerto, configBalanzaPuerto.LongFrase, configBalanzaPuerto.TimeoutLectura, Log, false, false);

            Log.Debug("Iniciando Driver de Balanza Puerto {0}", codigo);
            Regex soloNumerosBalanza = new Regex(@"[^-?\d]");
            if (!PalabrasReservadas.ContainsKey(codigo))
            {
                PalabrasReservadas.Add(codigoBalanzaPuerto + "TST2", "balanzada");
                PalabrasReservadas.Add(codigoBalanzaPuerto + "BATR", "balanzada");
                PalabrasReservadas.Add(codigo, soloNumerosBalanza.Replace(codigo, ""));
            }

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
                            //Log.Debug("Conexion reestablecida con la Balanza {0}", codigoBalanzaPuerto);
                            Log.Debug("Nueva Conexión a Balanza={0}", codigoBalanzaPuerto);
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al ConsultarBalanzada de la balanza {0}", codigoBalanzaPuerto);
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
                    {
                        cliente.ReConectar();
                    }
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
            try
            {
                var stringIdBalanzada = (IdBalanzada == null) ? "" : "," + IdBalanzada.ToString().PadLeft(configBalanzaPuerto.CantidadCaracteresTotal, configBalanzaPuerto.CaracterIzquierdaACompletar.First());
                var consulta = string.Empty;
                if (IdBalanzada.HasValue)
                {
                    consulta = GetObjectFromCache(IdBalanzada.Value + configBalanzaPuerto.Dispositivo.Codigo, Log);
                }

                if (string.IsNullOrEmpty(consulta))
                {
                    consulta = EnviarComando(configBalanzaPuerto.ComandoConsulta + stringIdBalanzada).TrimEnd('\0');
                }

                if (consulta != "NULL" || IdBalanzada.HasValue)
                {
                    Log.Info("Comando P," + IdBalanzada + " || Resultado: " + consulta);
                    try
                    {
                        if(!IdBalanzada.HasValue || ObtenerId(consulta) == IdBalanzada.Value)
                        {
                            Log.Info("Comando P," + IdBalanzada + " || voy a convertir");

                            var resultado = ConvertirADictionary(consulta, IdBalanzada);
                            Log.Info("Comando P," + IdBalanzada + " || Resultado: " + resultado.Count + "voy a cachear");

                            SetObjectToCache(ObtenerId(consulta) + configBalanzaPuerto.Dispositivo.Codigo, 24, consulta, Log);
                            Log.Info("Comando P," + IdBalanzada + " || salgo");

                            return resultado;
                        }
                        else
                        {
                            return Reintentar(++intento, consulta, IdBalanzada);
                        }
                    }
                    catch (Exception e)
                    {
                        return Reintentar(++intento, consulta, IdBalanzada);
                    }
                }

                return new Dictionary<string, string>();
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
        }

        private Dictionary<string,string> Reintentar(int intento, string consulta, int? IdBalanzada)
        {
            if (intento <= 5)
            {
                Log.Info("Reintento " + intento + "- " + IdBalanzada);
                Thread.Sleep(configBalanzaPuerto.IntervaloPolling + configBalanzaPuerto.IntervaloPolling * 1/3);
                return ConsultaBalanzada(IdBalanzada ?? ObtenerId(consulta), intento);
            }
            else
            {
                throw new FormatException(consulta);
            }
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
            if(id.Length == configBalanzaPuerto.CantidadCaracteresTotal)
            {
                return Convert.ToInt32(id);
            }
            else
            {
                throw new FormatException(consulta);
            }
            
        }

        public Dictionary<string, string> ConvertirADictionary(string consulta, int? IdBalanzada)
        {
            if (consulta.Contains(codigoBalanzaPuerto + " ERR"))
            {
                var respuesta = new Dictionary<string, string>() { { "tipoBalanzada", "error" + (consulta.Contains("MSC 41:UPD FACILITY ACTIVE") ? "41" : "") } };
                respuesta.Add("numeroBalanza", BalanzadaNombresDescriptivos(codigoBalanzaPuerto));
                var idConFechaError = consulta.Split(' ').GetValue(0).ToString() + consulta.Split(' ').GetValue(1).ToString();
                respuesta.Add("id", idConFechaError.Split(';').GetValue(0).ToString());
                respuesta.Add("fecha", idConFechaError.Split(';').GetValue(1).ToString().Trim().Replace(" ", ""));
                return respuesta;
            }
            else if (consulta.Contains(codigoBalanzaPuerto + "BATR") || consulta.Contains(codigoBalanzaPuerto + "TST2")) {
                consulta = consulta.Substring(0, consulta.IndexOf('h') + 1);
            }
            else {
                consulta = consulta.Substring(0, consulta.LastIndexOf('g') + 1);
            }

            var keys = Regex.Matches(consulta, @"(" + codigoBalanzaPuerto + "|TST1|STARCOM|TST2|BATRM|BATR|TST3|STORCOM|M:|START:|BOD.:|EXP.:|DES.:|BUQ.:|TNW:|TAW:|G:|MT:|T:|CAP:)")
                .Cast<Match>()
                .Select(m => BalanzadaNombresDescriptivos(m.Value))
                .ToList();

            var values = consulta.Split(PalabrasReservadas.Keys.ToArray(), StringSplitOptions.RemoveEmptyEntries)
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
                keys.RemoveAll( x => x == "balanzada");
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
    }
}
