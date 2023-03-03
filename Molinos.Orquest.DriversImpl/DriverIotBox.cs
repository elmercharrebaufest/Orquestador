using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverIotBox : DriverBase, IDriverItc
    {
        private readonly object lockComandoLectura = new object();
        private readonly object lockComandoEscritura = new object();

        private bool dispositivoActivo = false;
        private string codigoRasp;
        private ConfigItc config;

        private TcpCommandClient cliente;

        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.EntradaActivada, CodigosEventos.ErrorConexionDispositivo };

        public override bool MantenerConectado()
        {
            return true;
        }

        public override IEnumerable<string> EventosSoportados
        {
            get { return eventosSoportados; }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigItc); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoRasp = codigo;
            config = (ConfigItc)configuracion;
            dispositivoActivo = true;
            cliente = new TcpCommandClient(config.DireccionIp, config.Puerto, config.LongFrase, config.TimeoutLectura, Log, false);

            Log.Debug("Iniciando Driver de IotBox {0}", codigo);
            Task.Run(() =>
            {
                while (dispositivoActivo)
                {
                    try
                    {
                        ConsultarEstado();
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
                        {
                            Log.Debug("Conexion reestablecida con la Rasp {0}", codigoRasp);
                            Log.Info("Nueva Conexión a Rasp={0}", codigoRasp);
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al ConsultarEstado del Rasp {0}", codigoRasp);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de Rasp={0}", codigoRasp);
                            NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }
                        if (dispositivoActivo)
                        {
                            Thread.Sleep(config.IntervaloPolling);
                        }
                    }
                }
                finCiclo.Set();
            });
        }

        public override void VerificarDispositivo()
        {
            if ((falloUltimaConexion ?? false))
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
                    CodigoDispositivo = codigoRasp,
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
                Log.Error(ex, "Rasp {0}: No se pudo notificar el evento {1}", codigoRasp, codigoEvento);
            }
        }

        private void ConsultarEstado()
        {
            List<EntradaDto> respuesta = null;
            try
            {
                lock (lockComandoLectura)
                {
                    try
                    {
                        ActivarSalida(0, "\"ping\"", "0", false);
                    }
                    catch
                    {
                        cliente.ReConectar();
                        ActivarSalida(0, "\"socketconnected\"", "0", false);
                    }

                    string response;
                    try
                    {
                        response = cliente.LeerNovedad();
                    }
                    catch (SocketException e)
                    {
                        Log.Error("Error de conexion al leer respuesta, intentando un nuevo ping", e);
                        cliente.ReConectar();
                        ActivarSalida(0, "\"ping\"", "0", false);
                        response = cliente.LeerNovedad();
                    }

                    try
                    {
                        Log.Info("LECTURA - FRANCO: " + response);
                        if (response != null && response != "\"ok\"")
                        {
                            respuesta = JsonConvert.DeserializeObject<List<EntradaDto>>(response);
                            if (respuesta[0].Dato == "pingResponse")
                            {
                                Log.Info("Ping respondido exitosamente.");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al parsear respuesta");
                    }
                }
            }
            catch (Exception e) when (e.InnerException != null && (e.InnerException is SocketException) && ((SocketException)e.InnerException).ErrorCode == 10060)
            {
                Log.Debug(e, $"{codigoRasp} - Sin novedad");
                //throw new DriverException("El dispositivo no ha devuelto una respuesta", e);
            }
            catch (Exception e)
            {
                throw new DriverException("Error al Conectar con el dispositivo", e);
            }

            if (respuesta != null && respuesta[0].Dato != "pingResponse")
            {
                foreach (var entrada in respuesta)
                {
                    NotificarEventoEntrada(entrada.Numero, entrada.Dato, CodigosEventos.EntradaActivada);
                }
            }
        }

        public void ActivarSalida(int salida, string estado, string dato, bool flush = false)
        {
            Log.Debug("Activando Salida: ITC={0} Salida={1}", codigoRasp, salida);
            if (!dispositivoActivo)
            {
                return;
            }
            try
            {
                lock (lockComandoEscritura)
                {
                    if (!cliente.Conectado)
                    {
                        cliente.ReConectar();
                    }
                    cliente.EnviarComando("[{\"Tipo\": \"salida\", \"Numero\" : " + salida.ToString(CultureInfo.InvariantCulture) +
                        ", \"Dato\" : " + (estado == "1" ? "true" : (estado == "0" ? "false" : estado)) +
                        ", \"Delay\": " + dato + "}]", flush);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error al Conectar con el dispositivo", e);
                throw new DriverException("Error al Conectar con el dispositivo", e);
            }

            Log.Info("Salida Activada: ITC={0} Salida={1} Estado={2} Dato={3}", codigoRasp, salida, estado, dato);
        }

        public override void InformarEstado()
        {
            Log.Debug("Informando estado ITC {0}", codigoRasp);
            if (falloUltimaConexion.HasValue && falloUltimaConexion.Value)
            {
                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, errorUltimaConexion ?? new Exception(CodigosEventos.ErrorConexionDispositivo));
            }
            else
            {
                NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
            }
        }

        private void NotificarEventoEntrada(int entrada, string dato, string codigoEvento)
        {
            try
            {
                Log.Debug("Cambio Estado Entrada: Rasp={0} Entrada={1} Dato={3} Evento={2}", codigoRasp, entrada, codigoEvento, dato);

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoRasp,
                    CodigoEvento = codigoEvento,
                    Datos = new Dictionary<string, string>
                                {
                                    {"Entrada", entrada.ToString(CultureInfo.InvariantCulture)},
                                    {"Dato", dato}
                                }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se pudo notificar el evento ", codigoEvento);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cliente.Desconectar();
                dispositivoActivo = false;
                finCiclo.WaitOne();
                finCiclo.Dispose();
            }
        }

        public bool ConsultarEstadoEntrada(int numeroEntrada)
        {
            return false;
        }

        public void DesactivarSalida(int salida, string estado, string dato)
        {
            return;
        }

        public bool ConsultarEstadoActual(int numeroEntrada)
        {
            /// <summary>
            /// Motodo deshabilitado, falta realizar la implementación del lado del concetrador para el envío de un comando el cual permita consultar el estado actual de un sensor enviado como parámetro el código del dispositivo.
            /// </summary>
            
            return false;
        }

        public void NotificarEstadoActual(int numeroEntrada)
        {
            /// <summary>
            /// Metodo No implementado, falta realizar la implementación del lado del concetrador para el envío de un comando el cual permita consultar el estado actual de un sensor enviado como parámetro el código del dispositivo.
            /// </summary>
        }
    }
}