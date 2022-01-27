using AngleSharp.Common;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCartelLed_Multiled : DriverBase, IDriverCartelLed
    {
        private string codigoCartelLed;
        private ConfigCartelLed configuracionCarteLed;
        private string numeroTrama;
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCartelLed); }
        }

        public DriverCartelLed_Multiled()
        {
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoCartelLed = codigo;
            configuracionCarteLed = (ConfigCartelLed)configuracion;
            numeroTrama = "01";
        }

        public override bool MantenerConectado()
        {
            return true;
        }

        public override void VerificarDispositivo()
        {
            try
            {
                using (new  UdpCommandClient(configuracionCarteLed.DireccionIp, configuracionCarteLed.Puerto, configuracionCarteLed.LongFrase, configuracionCarteLed.TimeoutLectura, Log))
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
                using (var cliente = new UdpCommandClient(configuracionCarteLed.DireccionIp, configuracionCarteLed.Puerto, configuracionCarteLed.LongFrase, configuracionCarteLed.TimeoutLectura, Log))
                {
                    //0x30 cambiar programa actual
                    var programa = GenerarComando(null, 0x30, numeroPrograma);
                    cliente.EnviarComando(programa);

                    //0x3e seteo modo de reproduccion manual
                    var manual = GenerarComando(null, 0x3E, "00");
                    cliente.EnviarComando(manual);

                    //0x33 seleccionar pantalla dentro del programa actual
                    var step = GenerarComando(null, 0x33, numeroTrama);
                    cliente.EnviarComando(step);

                    //0x31 escribir una variable en la pantalla actual
                    var comando = GenerarComando(mensaje, 0x31, numeroVariable);
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

        public List<byte> GenerarComando(string dato, byte comando, string variable)
        {
            this.AumentarTrama(numeroTrama);
            List<byte> buffer = new List<byte>{ comando };

            if (!string.IsNullOrEmpty(variable))
            {
                buffer.Add(Convert.ToByte(variable[0]));
                buffer.Add(Convert.ToByte(variable[1]));
            }

            if (!string.IsNullOrEmpty(dato))
            {
                byte[] datos = Encoding.ASCII.GetBytes(dato);
                buffer.AddRange(datos);
            }

            buffer.Add(Convert.ToByte(numeroTrama[0]));
            buffer.Add(Convert.ToByte(numeroTrama[1]));


            var check = ObtenerChecksum(buffer.ToArray());
            buffer.AddRange(check);
            buffer.Add(0x10);
            buffer.Add(0x03);
            buffer.Insert(0, 0x10);
            buffer.Insert(0, 0x02);
            var a = ToHexString(buffer.ToArray());
            Log.Debug($"Cartel led{codigoCartelLed}, {a}");
            return buffer;
        }

        static readonly char[] _hexDigits = "0123456789abcdef".ToCharArray();
        public string ToHexString(byte[] bytes)
        {
            char[] digits = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int d1, d2;
                d1 = Math.DivRem(bytes[i], 16, out d2);
                digits[2 * i] = _hexDigits[d1];
                digits[2 * i + 1] = _hexDigits[d2];
            }
            return new string(digits);
        }

        private byte[] ObtenerChecksum(byte[] a)
        {
            var resultado = 0;
            foreach (var b in a)
            {
                resultado += b;
                resultado &= 0xFF;
            }
            resultado = (((resultado ^ 0XFF) + 1) & 0xFF);

            var LRC0 = (resultado & 0x0F);
            var LRC1 = (resultado >> 4);

            if (LRC0 >= 0 && LRC0 <= 9)
            {
                LRC0 = LRC0 + 48;
            }

            if (LRC0 >= 10 && LRC0 <= 15)
            {
                LRC0 = LRC0 + 55;
            }

            if (LRC1 >= 0 && LRC1 <= 9)
            {
                LRC1 = LRC1 + 48;
            }

            if (LRC1 >= 10 && LRC1 <= 15)
            {
                LRC1 = LRC1 + 55;
            }

            return new byte[] { Convert.ToByte(LRC1), Convert.ToByte(LRC0) };
        }

        private void AumentarTrama(string numeroTrama)
        {
            var numeroTramaInt = (Int32.Parse(numeroTrama) + 1);
            string numeroTramaNuevo;
            if (numeroTramaInt == 100)
            {
                numeroTramaNuevo = "01";
            }
            else if (numeroTramaInt < 10)
            {
                numeroTramaNuevo = string.Concat("0", numeroTramaInt);
            }
            else
            {
                numeroTramaNuevo = numeroTramaInt.ToString();
            }

            this.numeroTrama = numeroTramaNuevo;
        }

    }
}
