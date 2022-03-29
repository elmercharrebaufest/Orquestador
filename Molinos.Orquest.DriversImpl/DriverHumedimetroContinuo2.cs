using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroContinuo2 : DriverBase, IDriverHumedimetro
    {
        private string codigoHumedimetro;
        private ConfigHumedimetro configHumedimetro;
        private bool tomaHumedad;
        private bool pingOK;
        private bool conectado;
        private decimal? ultimaHumedadMedida;
        public decimal? UltimaPesoHectolitricoMedida;
        private DateTime? fechaDeMuestra;
        private TcpCommandClient cliente;
        private readonly object lockObject = new object();

        public override Type TipoDispositivo
        {
            get { return typeof (ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoHumedimetro = codigo;
            configHumedimetro = (ConfigHumedimetro) configuracion;
            tomaHumedad = true;
            pingOK = true;
            cliente = new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto,configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log);
            Task.Run(() =>
                {
                    while (tomaHumedad)
                    {
                        try
                        {

                            if (!cliente.Conectado || (pingOK && !conectado ))
                            {
                                ultimaHumedadMedida = null;
                                UltimaPesoHectolitricoMedida = null;
                                fechaDeMuestra = null;
                                cliente.ReConectar();
                                conectado = true;
                            }
                            var frase = cliente.LeerRespuestaHasta(10, configHumedimetro.LongFrase);
                            if (frase != null)
                            {
                                Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + frase.Replace("\r", "") + "'");
                                var arrayHumedad = frase.Split(new[] {configHumedimetro.DelimitadorCampos}, StringSplitOptions.None);
                                Log.Error("Lectura!! {0}", frase);
                                if (arrayHumedad.Length != 15)
                                {
                                    throw new FormatException(frase);
                                }
                                string stringHumedad = arrayHumedad[configHumedimetro.PosicionCampoHumedad];
                                string stringPH = arrayHumedad[configHumedimetro.PosicionCampoPesoHectolitrico];
                                lock (lockObject)
                                {
                                    ultimaHumedadMedida = decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);
                                    UltimaPesoHectolitricoMedida = decimal.Parse(stringPH, CultureInfo.InvariantCulture);
                                    fechaDeMuestra = DateTime.Now;
                                }
                            }
                        }
                        catch (SocketException e)
                        {
                            Log.Error(e, "Falló la conexión al dispositivo {0}", codigoHumedimetro);
                            conectado = false;
                            Thread.Sleep(1000);
                        }
                        catch (IOException)
                        {
                            //Log.Error(e, "Falló la conexión al dispositivo {0}", codigoHumedimetro);3
                            conectado = false;
                        }
                        catch (FormatException e)
                        {
                            Log.Error(e, "Formato de respuesta del dispositivo {0}", codigoHumedimetro);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error al conectarse al dispositivo {0}", configHumedimetro);
                            Thread.Sleep(1000);
                            conectado = false;
                        }
                    }
                });
            Task.Run(() =>
                {
                    while (tomaHumedad)
                    {
                        pingOK = PingOK(configHumedimetro.DireccionIp);
                        Thread.Sleep(pingOK ? 5000 : configHumedimetro.TimeoutLectura);
                    }
                });
        }

        public override void VerificarDispositivo()
        {
            if (cliente == null || !cliente.Conectado)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configHumedimetro));
            }
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            if (cliente.Conectado == false) throw new ConexionDispositivoDriverException();

            decimal? ultimaHumedad = null;
            lock (lockObject)
            {
                if (fechaDeInicio.HasValue && fechaDeMuestra.HasValue && fechaDeMuestra > fechaDeInicio)
                {
                    ultimaHumedad = ultimaHumedadMedida;
                    ultimaHumedadMedida = null;
                    fechaDeMuestra = null;
                }
                
            }
            return ultimaHumedad;
        }

        public decimal? ObtenerPH(DateTime? fechaDeInicio = null)
        {
            Log.Info("Captura de PH - Metodo ObtenerPH - DriverHumedimetroContinuo2");
            {
                if (cliente.Conectado == false) throw new ConexionDispositivoDriverException();

                decimal? UltimoPesoHectolitrico = null;
                lock (lockObject)
                {
                    if (fechaDeInicio.HasValue && fechaDeMuestra.HasValue && fechaDeMuestra > fechaDeInicio)
                    {
                        UltimoPesoHectolitrico = UltimaPesoHectolitricoMedida;
                        UltimaPesoHectolitricoMedida = null;
                        fechaDeMuestra = null;
                    }

                }
                return UltimoPesoHectolitrico;
            }
        }

        public override bool MantenerConectado()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                tomaHumedad = false;
                cliente.Dispose();
            }
        }

        private bool PingOK(string ip)
        {
            var pingOptions = new PingOptions(128, true);
            using (var ping = new Ping())
            {
                var buffer = new byte[32];

                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(ip, 3000, buffer, pingOptions);

                        if (pingReply != null && pingReply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
            return false;
        } 

    }
}
