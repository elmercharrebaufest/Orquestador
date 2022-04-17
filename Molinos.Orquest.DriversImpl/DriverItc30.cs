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
    public class DriverItc30 : DriverBase, IDriverItc
    {
        private readonly object lockComando = new object();

        private bool notificaEventos = false;
        private string codigoItc;
        private ConfigItc configItc;

        private char carInicioFrase;
        private char carFinFrase;

        private TcpCommandClient cliente;

        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);

        private byte? estadoAnterior;
        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.LecturaTarjetaRecibida, CodigosEventos.EntradaActivada, CodigosEventos.ErrorConexionDispositivo };

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
            codigoItc = codigo;
            configItc = (ConfigItc)configuracion;
            carInicioFrase = char.Parse(configItc.CarInicioFrase);
            carFinFrase = char.Parse(configItc.CarFinFrase);

            notificaEventos = true;
            cliente = new TcpCommandClient(configItc.DireccionIp, configItc.Puerto, configItc.LongFrase, configItc.TimeoutLectura, Log, false);

            Log.Debug("Iniciando Driver de ITC {0}", codigo);
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
                            Log.Debug("Conexion reestablecida con el ITC {0}", codigoItc);
                            Log.Info("Nueva Conexión a ITC={0}", codigoItc);
                            NotificarEstadoConexion(CodigosEventos.ConexionDispositivoCorrecta);
                            falloUltimaConexion = false;
                            errorUltimaConexion = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al ConsultarEstado del ITC {0}", codigoItc);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de ITC={0}", codigoItc);
                            NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }
                    }
                    Thread.Sleep(configItc.IntervaloPolling);
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
                    CodigoDispositivo = codigoItc,
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
                Log.Error(ex, "ITC {0}: No se pudo notificar el evento {1}", codigoItc, codigoEvento);
            }
        }

        private void ConsultarEstado()
        {
            var comando = Delimitar(string.Format("01 {0}", configItc.ComandoEstado));
            var respuesta = EnviarComando(comando, 1);
            if (respuesta.Length > 0)
            {
                var byteRespuesta = byte.Parse(respuesta, NumberStyles.HexNumber, CultureInfo.InstalledUICulture);

                if (byteRespuesta.BitAt(0))
                {
                    LeerTarjeta("1");
                }
                if (byteRespuesta.BitAt(1))
                {
                    LeerTarjeta("2");
                }

                for (int i = 4; i <= 7; i++)
                {
                    //Chequeamos el cambio de estado de desactivada a activada
                    if (byteRespuesta.BitAt(i) && (estadoAnterior.HasValue && !estadoAnterior.Value.BitAt(i)))
                    {
                        Log.Info("Entrada Activada: ITC={0} Entrada={1}", codigoItc, i - 3);
                        NotificarEventoEntrada(i - 3, CodigosEventos.EntradaActivada);
                        NotificarEventoEntrada(i - 3, CodigosEventos.CambioEstadoSensor, "true");
                    }
                    //chequeamos al cambio de estado de activada a desactivada
                    if (!byteRespuesta.BitAt(i) && (estadoAnterior.HasValue && estadoAnterior.Value.BitAt(i)))
                    {
                        Log.Info("Entrada Desactivada: ITC={0} Entrada={1}", codigoItc, i - 3);
                        NotificarEventoEntrada(i - 3, CodigosEventos.EntradaDesactivada);
                        NotificarEventoEntrada(i - 3, CodigosEventos.CambioEstadoSensor, "false");
                    }
                }
                estadoAnterior = byteRespuesta;
            }
        }

        private void NotificarEventoEntrada(int entrada, string codigoEvento)
        {
            try
            {
                Log.Debug("Cambio Estado Entrada: ITC={0} Entrada={1} Evento={2}", codigoItc, entrada, codigoEvento);

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoItc,
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

        private void NotificarEventoEntrada(int entrada, string codigoEvento, string dato)
        {
            try
            {
                Log.Info("NotificarEventoEntradaCambioSensor : ITC={0} Entrada={1} Evento={2} Dato={3}", codigoItc, entrada, codigoEvento, dato);

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoItc,
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

        private void LeerTarjeta(string lector)
        {
            Log.Debug("Leyendo tarjeta: ITC={0} Lector={1}", codigoItc, lector);
            var comando = Delimitar(string.Format("01 {0} {1}", configItc.ComandoTarjeta, lector));
            var tarjeta = EnviarComando(comando, 2);
            Log.Info("Tarjeta Leída: ITC={0} Lector={1} Tarjeta={2}", codigoItc, lector, tarjeta);

            if (tarjeta.Length == 0)
            {
                throw new DriverException(string.Format("No se pudo leer la Tarjeta con el Lector {0}", lector));
            }

            NotificarLecturaTarjeta(lector, tarjeta);
        }

        private void NotificarLecturaTarjeta(string lector, string tarjeta)
        {
            try
            {
                Log.Debug("Tarjeta Leída: ITC={0} Lector={1} NumeroTarjeta={2}", codigoItc, lector, tarjeta);
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoItc,
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>
                            {
                                {"Tarjeta", tarjeta},
                                {"Lector", lector}
                            }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception e)
            {
                Log.Error(e, "No se pudo notificar el evento ", CodigosEventos.LecturaTarjetaRecibida);
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
            Log.Debug("Activando Salida: ITC={0} Salida={1}", codigoItc, salida);
            var comando = Delimitar(string.Format("01 {0} {1} {2} {3}", configItc.ComandoActivarSalida, salida, estado, dato));
            var respuesta = EnviarComando(comando, 1);
            if (respuesta != configItc.RespuestaExito)
            {
                Log.Info("Salida No Activada: ITC={0} Salida={1}", codigoItc, salida);
                throw new DriverException(string.Format("No se pudo activar la salida {0}", salida));
            }
            Log.Info("Salida Activada: ITC={0} Salida={1}", codigoItc, salida);
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
            string frase;
            try
            {
                lock (lockComando)
                {
                    if (!cliente.Conectado)
                    {
                        cliente.ReConectar();
                    }
                    frase = cliente.EnviarComando(comando, 0, configItc.LongFrase);
                }
            }
            catch (Exception e)
            {
                throw new DriverException("Error al Conectar con el dispositivo", e);
            }

            return frase.Length > 0 ? frase.Substring(frase.IndexOf(carInicioFrase) + 1, frase.IndexOf(carFinFrase) - 1) : "";
        }

        private string EnviarComando(string comando, int campoRespuesta)
        {
            Log.Debug("ITC {0} Enviando Comando: {1}", codigoItc, comando);
            var frase = EnviarComando(comando);
            Log.Debug("ITC {0} Respuesta Comando: {1}", codigoItc, frase);
            if (!LecturaValida(frase))
            {
                Log.Warn("La respuesta al comando no pasó la validación de checksum. Descartando lectura..");
                return string.Empty;
            }
            var arrayFrase = frase.Split(new[] { configItc.DelimitadorCampos }, StringSplitOptions.None);
            return arrayFrase.Count() > campoRespuesta ? arrayFrase[campoRespuesta] : string.Empty;
        }

        private string Delimitar(string comando)
        {
            comando += " ";
            // "00 S 1 0 15 E6"
            var checksum = ((sbyte)-CalcularCheckSum(comando)).ToString("X2");
            return configItc.CarInicioFrase + comando + checksum + configItc.CarFinFrase + "\r\n";
        }

        private bool LecturaValida(string frase)
        {
            var valores = frase.Substring(0, frase.Length - 2);
            var checkSum = sbyte.Parse(frase.Substring(frase.Length - 2, 2), NumberStyles.HexNumber);
            var suma = CalcularCheckSum(valores);

            return (Convert.ToSByte(suma + checkSum) == 0);
        }

        private sbyte CalcularCheckSum(string stringComando)
        {
            sbyte suma = 0;
            foreach (var caracter in stringComando)
            {
                var valorHexa = Convert.ToSByte(caracter);
                suma = (sbyte)(suma + valorHexa);
            }
            return suma;
        }

        public override void InformarEstado()
        {
            Log.Debug("Informando estado ITC {0}", codigoItc);
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
            Log.Debug("Consultando Estado Entrada: ITC={0} Salida={1}", codigoItc, numeroEntrada);
            if (estadoAnterior == null)
            {
                ConsultarEstado();
            }
            if (estadoAnterior.HasValue && estadoAnterior.Value.BitAt(numeroEntrada + 3))
            {
                return true;
            }
            return false;
        }

        public void DesactivarSalida(int salida, string estado, string dato)
        {
            return;
        }

        public bool ConsultarEstadoActual(int numeroEntrada)
        {
            return ConsultarEstadoEntrada(numeroEntrada);
        }

        public void NotificarEstadoActual(int numeroEntrada)
        {
            try
            {
                Log.Info("NotificarEstadoActual: ITC={0} Salida={1}", codigoItc, numeroEntrada);
                bool estado = false;
                if (estadoAnterior == null)
                {
                    ConsultarEstado();
                }
                if (estadoAnterior.HasValue && estadoAnterior.Value.BitAt(numeroEntrada + 3))
                {
                    estado = true;
                    NotificarEventoEntrada(numeroEntrada, CodigosEventos.CambioEstadoSensor, estado.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    NotificarEventoEntrada(numeroEntrada, CodigosEventos.CambioEstadoSensor, estado.ToString(CultureInfo.InvariantCulture));
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error al NotificarEstadoActual del ITC {0}", codigoItc);
            }
        }
    }
}