using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroPorEventos : DriverBase, IDriverHumedimetro
    {
        private string codigoHumedimetro;
        private ConfigHumedimetro configHumedimetro;
        private bool notificaEventos;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return new[] {CodigosEventos.HumedadRecibida};
            }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoHumedimetro = codigo;
            configHumedimetro = (ConfigHumedimetro)configuracion;

            notificaEventos = true;
            Task.Run(() =>
                {
                    using (var cliente = new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto, configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log))
                    {
                        while (notificaEventos)
                        {
                            var frase = cliente.LeerRespuesta(0, configHumedimetro.LongFrase);
                            Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + frase.Replace("\r", "") + "'");
                            var stringHumedad =
                                frase.Split(new[] {configHumedimetro.DelimitadorCampos}, StringSplitOptions.None)[
                                    configHumedimetro.PosicionCampoHumedad];
                            var humedad = decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);
                            var notification = new NotificacionEvento
                                {
                                    CodigoDispositivo = codigoHumedimetro,
                                    CodigoEvento = CodigosEventos.HumedadRecibida,
                                    Valores =
                                        new Dictionary<string, decimal>
                                            {
                                                {"AnalisisHumedad", humedad}
                                            }
                                };

                            OnEventoDriver(new EventoDriverEventArgs {Notificacion = notification});
                        }
                    }
                });
        }

        public override void VerificarDispositivo()
        {
            try
            {
                using (new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto, configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log))
                {
                    // Intentamos conectarnos al dispositivo
                    // Se puede probar algo más?
                }
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configHumedimetro), e);
            }
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            try
            {
                using (var cliente = new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto, configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log))
                {
                    var frase = cliente.LeerRespuesta(0, configHumedimetro.LongFrase);
                    Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + (frase != null ? frase.Replace("\r", ""):"No Responde") + "'");
                    var stringHumedad = frase.Split(new[] { configHumedimetro.DelimitadorCampos }, StringSplitOptions.None)[configHumedimetro.PosicionCampoHumedad];
                    return decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoHumedimetro), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoHumedimetro), e);
            }
            catch (FormatException e)
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0}", codigoHumedimetro), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configHumedimetro), e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notificaEventos = false;
            }
        }
    }
}
