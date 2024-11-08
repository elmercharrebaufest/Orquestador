using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public sealed class TcpCommandClient : IDisposable
    {
        private TcpClient clienteTcp;
        private readonly string host;
        private readonly int puerto;
        private readonly int tamBuffer;
        private readonly int timeoutLectura;
        private readonly ILogger log;
        private readonly bool loguear;
        private NetworkStream networkStream;

        public TcpCommandClient(string host, int puerto, int tamBuffer, int timeoutLectura, ILogger log, bool conectar = true, bool loguear = true)
        {
            this.host = host;
            this.puerto = puerto;
            this.tamBuffer = tamBuffer;
            this.timeoutLectura = timeoutLectura;
            this.log = log;
            this.loguear = loguear;
            if (conectar)
            {
                Conectar();
            }
        }

        private void Conectar()
        {
			clienteTcp = new TcpClient(host, puerto) { ReceiveBufferSize = tamBuffer };
            if (timeoutLectura > 0)
            {
                clienteTcp.GetStream().ReadTimeout = timeoutLectura;
            }
        }

        public bool Conectado
        {
            get { return clienteTcp != null && clienteTcp.Connected; }
        }

        public void ReConectar()
        {
			if (clienteTcp != null)
            {
                clienteTcp.Close();
			}
            Conectar();
        }

		public void Liberar()
		{
			if (clienteTcp != null)
			{
				clienteTcp.Dispose();
			}			
		}

		public string EnviarComando(string comando, int posInicioRespuesta, int posFinRespuesta)
        {
            var netStream = clienteTcp.GetStream();
            byte[] writeBuffer = Encoding.ASCII.GetBytes(comando);
            netStream.Write(writeBuffer, 0, writeBuffer.Length);

            var readBuffer = new byte[clienteTcp.ReceiveBufferSize];
            netStream.Read(readBuffer, 0, readBuffer.Length);
            if (comando == "P" && loguear)
            {
                log.Info("Comando - " + comando + " - Dispositivo: " + host + ":" + puerto.ToString() + " Respuesta: '" + (Encoding.ASCII.GetString(readBuffer.ToArray()).Replace("\r", "")) + "'");
            }

            return Encoding.ASCII.GetString(readBuffer, posInicioRespuesta, posFinRespuesta - posInicioRespuesta);
        }
        public string EnviarComandoHex(byte[] writeBuffer)
        {
            var netStream = clienteTcp.GetStream();
            netStream.Write(writeBuffer, 0, writeBuffer.Length);

            var readBuffer = new byte[clienteTcp.ReceiveBufferSize];
            var readBytesCount = netStream.Read(readBuffer, 0, readBuffer.Length);

            byte[] truncArray = new byte[readBytesCount];
            Array.Copy(readBuffer, truncArray, truncArray.Length);
            return BitConverter.ToString(truncArray);
        }

        //public void EnviarComando(string comando)
        //{
        //    var netStream = clienteTcp.GetStream();
        //    byte[] writeBuffer = Encoding.ASCII.GetBytes(comando);
        //    netStream.Write(writeBuffer, 0, writeBuffer.Length);
        //}

        public void EnviarComando(string comando, bool flush = false)
        {
            var netStream = clienteTcp.GetStream();
            byte[] writeBuffer = Encoding.ASCII.GetBytes(comando);
            netStream.Write(writeBuffer, 0, writeBuffer.Length);
            if (flush)
            {
                netStream.Flush();
            }
        }

        public void EnviarComando(List<byte> writeBuffer)
        {
            var netStream = clienteTcp.GetStream();
            netStream.Write(writeBuffer.ToArray(), 0, writeBuffer.Count);
        }
        public void EnviarComando(byte[] writeBuffer)
        {
            var netStream = clienteTcp.GetStream();
            netStream.Write(writeBuffer.ToArray(), 0, writeBuffer.Length);
        }
        public string LeerRespuesta(int posInicioRespuessta, int posFinRespuesta)
        {
            var netStream = clienteTcp.GetStream();
            var readBuffer = new byte[clienteTcp.ReceiveBufferSize];
            netStream.Read(readBuffer, 0, readBuffer.Length);

            return Encoding.ASCII.GetString(readBuffer, posInicioRespuessta, posFinRespuesta - posInicioRespuessta);
        }

        public string LeerRespuestaHasta(byte fin, int longitudDeFrase)
        {
            var netStream = clienteTcp.GetStream();
            var readBuffer = new List<byte>();

            var read = netStream.ReadByte();
            if (read == -1)
            {
                clienteTcp.Close();
            }
            var attempt = 0;
            while (((byte)read) != fin && attempt < longitudDeFrase)
            {
                readBuffer.Add((byte)read);
                read = netStream.ReadByte();
                attempt++;
            }
            return attempt == longitudDeFrase ? null : Encoding.ASCII.GetString(readBuffer.ToArray());
        }

        public string LeerRespuestaContinua(char[] delimInicioFrase, int posInicioRespuesta, int posFinRespuesta)
        {
            var netStream = clienteTcp.GetStream();
            int read = netStream.ReadByte();
            int attempt = 0;
            log.Debug("LeerRespuestaContinua delim {0}, posInicioRespuesta {1} , posFinRespuesta {2}", String.Join(",", delimInicioFrase), posInicioRespuesta, posFinRespuesta);
            while (!delimInicioFrase.Any(x => x == ((char)read)) && attempt < 100)
            {
                log.Debug("Read: {0} / {1}, attempt {2}", ((char)read), read, attempt);
                read = netStream.ReadByte();
                attempt++;
            }
            if (attempt == 100)
            {
                return null;
            }
            var readBuffer = new byte[clienteTcp.ReceiveBufferSize];
            int readCount = netStream.Read(readBuffer, 0, readBuffer.Length);
            log.Debug("Lectura " + Encoding.ASCII.GetString(readBuffer));
            log.Info("Lectura Respuesta Continua - Respuesta: '" + (Encoding.ASCII.GetString(readBuffer.ToArray()).Replace("\r", "")) + "'");
            posInicioRespuesta = posInicioRespuesta - 1;
            return readCount < readBuffer.Length ? null : Encoding.ASCII.GetString(readBuffer, posInicioRespuesta, posFinRespuesta - posInicioRespuesta);
        }

        public string LeerNovedad()
        {
            var serverMessage = string.Empty;
            Byte[] bytes = new Byte[clienteTcp.ReceiveBufferSize];
            int length;
            networkStream = clienteTcp.GetStream();

            if (timeoutLectura > 0)
            {
                networkStream.ReadTimeout = timeoutLectura;
            }

            if ((length = networkStream.Read(bytes, 0, bytes.Length)) == 0)
            {
                clienteTcp = null;
                throw new SocketException();
            }
            var incommingData = new byte[length];
            Array.Copy(bytes, 0, incommingData, 0, length);
            // Convert byte array to string message. 						
            serverMessage = Encoding.ASCII.GetString(incommingData);
            log.Debug("server message received as: " + serverMessage);
            return serverMessage;
        }

        public void Desconectar()
        {
            Dispose();
        }

        public void Dispose()
        {
            var disposable = clienteTcp as IDisposable;
            if (disposable != null)
            {

                disposable.Dispose();
            }
            if (networkStream != null)
            {
                networkStream.Close();
            }
        }
    }
}
