using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCartelLed : DriverBase, IDriverCartelLed
    {
        private string codigoCartelLed;
        private ConfigCartelLed configuracionCarteLed;
        private string mensajeActual;
        private bool intervaloActivo;
        private DateTime fechaFinEjecucion;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCartelLed); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoCartelLed = codigo;
            configuracionCarteLed = (ConfigCartelLed)configuracion;
        }

        public override void VerificarDispositivo()
        {
            try
            {
                using (new TcpCommandClient(configuracionCarteLed.DireccionIp, configuracionCarteLed.Puerto, configuracionCarteLed.LongFrase, configuracionCarteLed.TimeoutLectura, Log))
                {
                    // Intentamos conectarnos al dispositivo
                    // Se puede probar algo más?
                }
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoCartelLed), e);
            }
        }

        public void EnviarMensaje(string mensaje, string numeroPrograma, string numeroTrama, string numeroVariable)
        {
            try
            {
                using (var cliente = new TcpCommandClient(configuracionCarteLed.DireccionIp, configuracionCarteLed.Puerto, configuracionCarteLed.LongFrase, configuracionCarteLed.TimeoutLectura, Log))
                {
                    byte[] comandoSinTexto = { 0x31, 0x54, Convert.ToByte(configuracionCarteLed.VelocidadScroll), Convert.ToByte(configuracionCarteLed.Tipografia), Convert.ToByte(configuracionCarteLed.ControlBrillo), Convert.ToByte(configuracionCarteLed.Efecto) };
                    byte[] texto = Encoding.ASCII.GetBytes(mensaje);
                    Log.Info(configuracionCarteLed.Dispositivo.Codigo);
                    byte[] comandoConTexto = comandoSinTexto.Concat(texto).ToArray();
                    List<byte> comando = comandoConTexto.ToList();
                    var check = ObtenerChecksum(comandoConTexto);
                    comando.Add(0x03);
                    comando.Add(check);
                    comando.Insert(0, 0x02);
                    comando.Insert(0, 0x02);
                    cliente.EnviarComando(comando);
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", configuracionCarteLed), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", configuracionCarteLed), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configuracionCarteLed), e);
            }
        }

        private static byte ObtenerChecksum(byte[] a)
        {
            var resultado = 0;
            for (int i = 0; i < a.Length; i++)
            {
                resultado += a[i];

                Console.WriteLine(resultado);
            }
            resultado %= 256;

            if (resultado < 10) { resultado += 20; }
            return Convert.ToByte(resultado);
        }

        public void EnviarMensajeIntervalo(string textoPrimario, string textoSecundario, string numeroPrograma, string numeroTrama, string numeroVariable, int intervalMilliseconds)
        {
            intervaloActivo = true;
            fechaFinEjecucion = DateTime.Now.AddMinutes(30);
            Task.Run(() =>
            {
                while (intervaloActivo)
                {
                    if (DateTime.Now > fechaFinEjecucion)
                        intervaloActivo = false;

                    mensajeActual = (mensajeActual == textoPrimario) ? textoSecundario : textoPrimario;
                    EnviarMensaje(mensajeActual, numeroPrograma, numeroTrama, numeroVariable);
                    Thread.Sleep(intervalMilliseconds);
                }
            });
        }

        public void DetenerIntervalo()
        {
            intervaloActivo = false;
        }
    }
}