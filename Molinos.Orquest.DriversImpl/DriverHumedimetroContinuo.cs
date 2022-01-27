using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroContinuo : DriverBase, IDriverHumedimetro
    {
        private string codigoHumedimetro;
        private ConfigHumedimetro configHumedimetro;
        private bool tomaHumedad;
        private decimal? ultimaHumedadMedida;
        private DateTime? fechaDeMuestra;
        private TcpCommandClient cliente;
        private readonly object lockObject = new object();

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoHumedimetro = codigo;
            configHumedimetro = (ConfigHumedimetro)configuracion;
            tomaHumedad = true;
            cliente = new TcpCommandClient(configHumedimetro.DireccionIp, configHumedimetro.Puerto, configHumedimetro.LongFrase, configHumedimetro.TimeoutLectura, Log);
            Task.Run(() =>
            {
                while (tomaHumedad)
                {
                    try
                    {
                        if (!cliente.Conectado)
                        {
                            ultimaHumedadMedida = null;
                            fechaDeMuestra = null;
                            cliente.ReConectar();
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
                        Thread.Sleep(1000);
                    }
                    catch (IOException)
                    {
                        //Log.Error(e, "Falló la conexión al dispositivo {0}", codigoHumedimetro);
                    }
                    catch (FormatException e)
                    {
                        Log.Error(e, "Formato de respuesta del dispositivo {0}", codigoHumedimetro);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al conectarse al dispositivo {0}", configHumedimetro);
                        Thread.Sleep(1000);
                    }
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
    }
}
