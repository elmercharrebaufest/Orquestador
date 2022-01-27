using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetro : DriverBase, IDriverHumedimetro
    {
        private string codigoHumedimetro;
        private ConfigHumedimetro configHumedimetro;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoHumedimetro = codigo;
            configHumedimetro = (ConfigHumedimetro)configuracion;
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
                    var frase = cliente.LeerRespuestaHasta(10, configHumedimetro.LongFrase);
                    Log.Info("Captura de Humedad - " + configHumedimetro.Dispositivo.Codigo + " - '" + (frase != null ? frase.Replace("\r", ""):"No Responde") + "'");
                    var arrayHumedad = frase.Split(new[] {configHumedimetro.DelimitadorCampos}, StringSplitOptions.None);
                    if (arrayHumedad.Length != 15)
                    {
                        throw new FormatException(frase);
                    }
                    var stringHumedad = arrayHumedad[configHumedimetro.PosicionCampoHumedad];
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
    }
}
