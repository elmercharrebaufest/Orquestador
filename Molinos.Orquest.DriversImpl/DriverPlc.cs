using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverPlc : DriverBase, IDriverItc
    {
        private readonly object lockComando = new object();

        private bool notificaEventos = false;
        private string codigoPlc;
        private ConfigItc config;
        private int transaccion;
        private char carInicioFrase;
        private char carFinFrase;

        private TcpCommandClient cliente;

        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        private byte[] estadoAnterior;
        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.LecturaTarjetaRecibida, CodigosEventos.EntradaActivada, CodigosEventos.ErrorConexionDispositivo, CodigosEventos.CambioEstadoSensor };

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
            transaccion = 0;
            codigoPlc = codigo;
            config = (ConfigItc)configuracion;
            carInicioFrase = char.Parse(config.CarInicioFrase);
            carFinFrase = char.Parse(config.CarFinFrase);

            notificaEventos = true;
            cliente = new TcpCommandClient(config.DireccionIp, config.Puerto, config.LongFrase, config.TimeoutLectura, Log, false);

            Log.Debug("Iniciando Driver de PLC {0}", codigo);
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
                            Log.Debug("Conexion reestablecida con el PLC {0}", codigoPlc);
                            Log.Debug("Nueva Conexión a PLC={0}", codigoPlc);
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al ConsultarEstado del PLC {0}", codigoPlc);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Debug("Desconexión de PLC={0}", codigoPlc);
                            NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }
                    }
                    Thread.Sleep(config.IntervaloPolling);
                }
                finCiclo.Set();
            });
        }

        private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
        {
            try
            {
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoPlc,
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
                Log.Error(ex, "PLC {0}: No se pudo notificar el evento {1}", codigoPlc, codigoEvento);
            }
        }

        private void ConsultarEstado()
        {
            var cantidadDeBytes = 64.ToString("X2").PadLeft(4, '0');
            var comando = ArmarComandoConsulta(config.ComandoEstado.PadLeft(2, '0'), "0000", cantidadDeBytes);
            var respuesta = EnviarComando(comando);
            Log.Debug($"Consulta Entrada: PLC={codigoPlc} Comando={comando} Respuesta ={respuesta}");
            ProcesarRespuesta(respuesta);
        }

        private void ProcesarRespuesta(string respuesta)
        {
            if (respuesta.Length > 0)
            {
                var bytesRespuesta = StringToByteArray(respuesta.Replace("-", ""));
                Log.Debug("Consulta Entrada: PLC={0} Bytes={1}", codigoPlc, bytesRespuesta.Count());
                var largoTrama = 9;
                var cantidadDeBytesConDatos = bytesRespuesta[largoTrama - 1];
                var entrada = 0;
                if ((estadoAnterior == null && bytesRespuesta != null) || bytesRespuesta[largoTrama] != estadoAnterior[largoTrama])
                {
                    var datos = bytesRespuesta.ToList();
                    datos.RemoveRange(0, largoTrama);

                    var texto = new List<string>();
                    foreach (var item in datos)
                    {
                        texto.Add(item.ToString("X").PadLeft(2, '0'));
                    }
                    NotificarEventoEntrada(0, CodigosEventos.CambioEstadoSensor, string.Join("-", texto));
                }
                if (bytesRespuesta.Length > largoTrama && cantidadDeBytesConDatos > 0)
                {
                    var indiceFinalDeDatos = cantidadDeBytesConDatos + largoTrama - 1;
                    for (int byteIndex = largoTrama; byteIndex <= indiceFinalDeDatos; byteIndex++)
                    {
                        var byteActual = bytesRespuesta[byteIndex];
                        if (estadoAnterior != null && estadoAnterior.Length >= byteIndex)
                        {
                            Log.Debug($"Valor Byte Entrada: {estadoAnterior[byteIndex]} Byte Nuevo: {byteActual}");
                        }
                        for (int bitIndex = 7; bitIndex >= 0; bitIndex--)
                        {
                            var valorBite = byteActual.BitAt(bitIndex);
                            Log.Debug($"Entrada: PLC={codigoPlc} ByteNro={byteIndex} BitNro={entrada} ValorByte={valorBite}");
                            //Chequeamos el cambio de estado de desactivada a activada
                            if (valorBite && estadoAnterior != null && estadoAnterior.Length >= byteIndex && !estadoAnterior[byteIndex].BitAt(bitIndex))
                            {
                                Log.Debug("Actualizar Entrada Activada: PLC={0} Entrada={1} Valor={2}", codigoPlc, entrada, valorBite);

                                NotificarEventoEntrada(entrada, CodigosEventos.EntradaActivada);
                                NotificarEventoEntrada(0, CodigosEventos.CambioEstadoSensor, valorBite.ToString());
                            }
                            //chequeamos al cambio de estado de activada a desactivada
                            if (!valorBite && estadoAnterior != null && estadoAnterior.Length >= byteIndex && estadoAnterior[byteIndex].BitAt(bitIndex))
                            {
                                Log.Debug("Actualizar  Entrada Desactivada: PLC={0} Entrada={1} Valor={2}", codigoPlc, entrada, valorBite);
                                NotificarEventoEntrada(entrada, CodigosEventos.EntradaDesactivada);
                                NotificarEventoEntrada(0, CodigosEventos.CambioEstadoSensor, valorBite.ToString());
                            }
                            entrada++;
                        }
                    }
                }
                estadoAnterior = bytesRespuesta;
                Log.Debug("Bytes largo:{0} estadoAnterior: {1}", estadoAnterior.Length, string.Join(" ", estadoAnterior));
            }
        }

        private void NotificarEventoEntrada(int entrada, string codigoEvento)
        {
            try
            {
                Log.Info("Cambio Estado Entrada: PLC={0} Entrada={1} Evento={2}", codigoPlc, entrada, codigoEvento);

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoPlc,
                    CodigoEvento = codigoEvento,
                    Datos = new Dictionary<string, string>
                                {
                                    {"Entrada", entrada.ToString(CultureInfo.InvariantCulture)}
                                }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se pudo notificar el evento ", codigoEvento);
            }
        }

        private void NotificarEventoEntrada(int entrada, string codigoEvento, string mensaje)
        {
            try
            {
                Log.Info("Cambio Estado Entrada: PLC={0} Entrada={1} Evento={2}", codigoPlc, entrada, codigoEvento);

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoPlc,
                    CodigoEvento = codigoEvento,
                    Datos = new Dictionary<string, string>
                                {
                                    {"Entrada", entrada.ToString(CultureInfo.InvariantCulture)},
                                    {"Mensaje", mensaje}
                                }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
                Log.Info($"Notificacion enviada {notification}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se pudo notificar el evento ", codigoEvento);
            }
        }

        public override void VerificarDispositivo()
        {
            if (cliente == null || !cliente.Conectado)
            {
                throw new ConexionDispositivoDriverException("");
            }
        }

        public void ActivarSalida(int salida, string estado, string dato, bool flush = false)
        {
            EjecutarSalida(salida, "FF00");
        }

        public void DesactivarSalida(int salida, string estado, string dato)
        {
            EjecutarSalida(salida, "0000");
        }

        private void EjecutarSalida(int salida, string tipoComando)
        {
            var salidaHex = salida.ToString("X2").PadLeft(4, '0');
            Log.Info("Ejecutando Salida: PLC={0} Salida={1} SalidaBytes={2}", codigoPlc, salida, salidaHex);
            var comando = ArmarComandoConsulta(config.ComandoActivarSalida.PadLeft(2, '0'), salidaHex, tipoComando);
            Log.Debug("Ejecutando Salida: PLC={0} Comando={1} SalidaByte={2}", codigoPlc, comando, salidaHex);

            var respuesta = EnviarComando(comando);
            Log.Debug("Respuesta Salida: PLC={0} Comando={1}", codigoPlc, respuesta);

            if (respuesta.Replace("-", "").Substring(0, comando.Length) != comando)
            {
                Log.Debug("Salida No Activada: PLC={0} Salida={1}", codigoPlc, salida);
                throw new DriverException(string.Format("No se pudo activar la salida {0}", salida));
            }
            Log.Debug("Salida Activada: PLC={0} Salida={1}", codigoPlc, salida);
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

        private string EnviarComando(string comando)
        {
            string respuesta;
            try
            {
                lock (lockComando)
                {
                    if (!cliente.Conectado)
                    {
                        cliente.ReConectar();
                    }
                    respuesta = cliente.EnviarComandoHex(StringToByteArray(comando));
                }
            }
            catch (Exception e)
            {
                throw new DriverException("Error al Conectar con el dispositivo", e);
            }

            return respuesta;
        }

        public override void InformarEstado()
        {
            Log.Debug("Informando estado PLC {0}", codigoPlc);
            if (falloUltimaConexion.HasValue && falloUltimaConexion.Value)
            {
                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, errorUltimaConexion ?? new Exception(CodigosEventos.ErrorConexionDispositivo));
            }
            else
            {
                NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
            }
        }

        public bool ConsultarEstadoEntrada(int numeroEntrada)
        {
            Log.Debug("Consultando Estado Entrada: PLC={0} Salida={1}", codigoPlc, numeroEntrada);

            if (estadoAnterior == null)
            {
                ConsultarEstado();
            }
            if (estadoAnterior != null && estadoAnterior.Length > 9)
            {
                var bytePosicion = 9 + (numeroEntrada / 8);
                var bitPosicion = 7 - (numeroEntrada % 8);
                var valor = estadoAnterior[bytePosicion].BitAt(bitPosicion);
                Log.Debug("Estado Entrada: PLC={0} en Byte={1} Bit={2} Valor={3}", codigoPlc, bytePosicion, bitPosicion, valor);
                return valor;
            }
            return false;
        }

        private string ArmarComandoConsulta(string codigoEvento, string inicio, string registro)
        {
            var transaccionId = transaccion.ToString("X").PadLeft(4, '0');
            transaccionId = transaccionId.Substring(transaccionId.Length - 4);
            transaccion++;
            //transaccionId + protocol(000000) + largo(06) + unitId(01) + codigoEvento + inicio + registro
            return $"{transaccionId}0000000601{codigoEvento}{inicio}{registro}";
        }

        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}