using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroPHContinuoDummy : DriverBase, IDriverHumedimetro
    {
        private readonly object lockObject = new object();
        private TcpCommandClient cliente;
        private string codigoHumedimetro;
        private bool conectado;
        private ConfigHumedimetro configHumedimetro;
        private DateTime? fechaDeMuestra;
        private bool pingOK;
        private bool tomaHumedad;
        private decimal? ultimaHumedadMedida;
        private decimal? ultimaPHMedida;
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoHumedimetro = codigo;
            configHumedimetro = (ConfigHumedimetro)configuracion;
            tomaHumedad = true;
            pingOK = true;
            cliente = new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto, configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log);
            Task.Run(() =>
            {
                while (tomaHumedad)
                {
                    try
                    {
                        if (!cliente.Conectado || (pingOK && !conectado))
                        {
                            ultimaHumedadMedida = null;
                            ultimaPHMedida = null;
                            fechaDeMuestra = null;
                            cliente.ReConectar();
                            conectado = true;
                        }
                        //var frase = cliente.LeerRespuestaHasta(10, configHumedimetro.LongFrase);
                        string frase = " ,29/03/22,14:20:14,11.9,70.7,30.3,28.9,29.7,SOJA ARG,S/N: 1716-32552, 2, 895,2305, 303,070815";
                        if (frase != null)
                        {
                            //Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + frase.Replace("\r", "") + "'");
                            var arrayHumedad = frase.Split(new[] { configHumedimetro.DelimitadorCampos }, StringSplitOptions.None);
                            //Log.Error("Lectura!! {0}", frase);
                            if (arrayHumedad.Length != 15)
                            {
                                throw new FormatException(frase);
                            }
                            string stringHumedad = arrayHumedad[configHumedimetro.PosicionCampoHumedad];
                            string stringPH = arrayHumedad[configHumedimetro.PosicionCampoPesoHectolitrico];
                            lock (lockObject)
                            {
                                ultimaHumedadMedida = decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);
                                ultimaPHMedida = decimal.Parse(stringPH, CultureInfo.InvariantCulture);
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
                    catch (IOException e)
                    {
                        Log.Error(e, "Error I/O al conectarse al dispositivo {0}", codigoHumedimetro);
                        conectado = false;
                    }
                    catch (FormatException e)
                    {
                        Log.Error(e, "Formato de respuesta del dispositivo {0}", codigoHumedimetro);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al conectarse al dispositivo {0}", configHumedimetro);
                        conectado = false;
                        Thread.Sleep(1000);
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

        public override bool MantenerConectado()
        {
            return true;
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            return null;
        }

        public HumedimetroResultadoDto ObtenerHumedadPH(DateTime? fechaDeInicio = null)
        {
            var humedimetroResultado = new HumedimetroResultadoDto();
            lock (lockObject)
            {
                if (fechaDeInicio.HasValue && fechaDeMuestra.HasValue && fechaDeMuestra > fechaDeInicio)
                {
                    humedimetroResultado = new HumedimetroResultadoDto
                    {
                        Humedad = Convert.ToDecimal(ultimaHumedadMedida),
                        PH = Convert.ToDecimal(ultimaPHMedida)
                    };
                    ultimaHumedadMedida = null;
                    ultimaPHMedida = null;
                    fechaDeMuestra = null;
                }
            }
            return humedimetroResultado;
        }

        public override void VerificarDispositivo()
        {
            if (cliente == null || !cliente.Conectado)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configHumedimetro));
            }
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
                        Log.Error(e, "Error validaction de conexion");
                    }
                }
            }
            return false;
        }
    }
}