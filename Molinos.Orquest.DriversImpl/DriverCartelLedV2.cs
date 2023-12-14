using Molinos.Orquest.Dominio.Dtos;
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
    public class DriverCartelLedV2 : DriverBase, IDriverCartelLed
    {
        private string codigoCartelLed;
        private ConfigCartelLed configuracionCarteLed;
        private List<MensajeIntervaloDto> mensajeIntevaloList = new List<MensajeIntervaloDto>();

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
            RemoverMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);
            var fechaFinEjecucion = DateTime.Now.AddMinutes(30);
            AgregarMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable, intervalMilliseconds);
            Task.Run(() =>
            {
                while (ObtenerMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable) != null)
                {
                    if (DateTime.Now > fechaFinEjecucion)
                    {
                        RemoverMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);
                        break;
                    }

                    var mensajeObtenido = ObtenerMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);
                    mensajeObtenido.MensajeActual = (mensajeObtenido.MensajeActual == textoSecundario) ? textoPrimario : textoSecundario;
                    EnviarMensaje(mensajeObtenido.MensajeActual, mensajeObtenido.NumeroPrograma, mensajeObtenido.NumeroTrama, mensajeObtenido.NumeroVariable);
                    Thread.Sleep(mensajeObtenido.Intervalo);
                }
            });
        }

        public void DetenerIntervalo(string numeroPrograma, string numeroTrama, string numeroVariable)
        {
            RemoverMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);
        }

        private MensajeIntervaloDto ObtenerMensajeIntevalo(string numeroPrograma, string numeroTrama, string numeroVariable)
        {
            var mensajeIntervalo = mensajeIntevaloList.FirstOrDefault(q => q.NumeroPrograma == numeroPrograma && q.NumeroTrama == numeroTrama && q.NumeroVariable == numeroVariable);
            return mensajeIntervalo;
        }

        private void RemoverMensajeIntevalo(string numeroPrograma, string numeroTrama, string numeroVariable)
        {
            var mensaje = ObtenerMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);
            if (mensaje != null)
            {
                mensajeIntevaloList.Remove(mensaje);
                Log.Info($"DriverCartelLedV2  Se removio mensaje de lista: {codigoCartelLed}, {mensajeIntevaloList.Count}");
            }
        }

        private void AgregarMensajeIntevalo(string numeroPrograma, string numeroTrama, string numeroVariable, int intervalMilliseconds)
        {
            var mensaje = ObtenerMensajeIntevalo(numeroPrograma, numeroTrama, numeroVariable);

            if (mensaje == null)
            {
                mensaje = new MensajeIntervaloDto
                {
                    NumeroPrograma = numeroPrograma,
                    NumeroTrama = numeroTrama,
                    NumeroVariable = numeroVariable,
                    Intervalo = intervalMilliseconds
                };
                mensajeIntevaloList.Add(mensaje);
                Log.Info($"DriverCartelLedV2  Se agrego mensaje a lista: {codigoCartelLed}, {mensajeIntevaloList.Count}");
            }
        }
    }
}