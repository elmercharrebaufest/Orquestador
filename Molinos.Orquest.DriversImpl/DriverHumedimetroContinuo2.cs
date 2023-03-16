using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.Helpers;
using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroContinuo2 : DriverBase, IDriverHumedimetro
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
                        if (ConnectionHelper.IsConnected(cliente, pingOK,conectado))
                        {
                            ultimaHumedadMedida = null;
                            fechaDeMuestra = null;
                            cliente.ReConectar();
                            conectado = true;
                        }
                        var frase = cliente.LeerRespuestaHasta(10, configHumedimetro.LongFrase);
                        if (frase != null)
                        {
                            Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + frase.Replace("\r", "") + "'");
                            var arrayHumedad = frase.Split(new[] { configHumedimetro.DelimitadorCampos }, StringSplitOptions.None);
                            Log.Error("Lectura!! {0}", frase);
                            if (arrayHumedad.Length != 15)
                            {
                                throw new FormatException(frase);
                            }
                            var stringHumedad = arrayHumedad[configHumedimetro.PosicionCampoHumedad];
                            lock (lockObject)
                            {
                                ultimaHumedadMedida = decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);
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
                    pingOK = ConnectionHelper.PingOK(configHumedimetro.DireccionIp);
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

        public HumedimetroResultadoDto ObtenerHumedadPH(DateTime? fechaDeInicio = null)
        {
            return null;
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
    }
}