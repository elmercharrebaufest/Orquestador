using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public sealed class UdpCommandClient : IDisposable
    {
        private UdpClient clienteUdp;
        private readonly string host;
        private readonly int puerto;
        private readonly int tamBuffer;
        private readonly int timeoutLectura;
        private readonly ILogger log;
        private readonly bool loguear;

        public UdpCommandClient(string host, int puerto, int tamBuffer, int timeoutLectura, ILogger log, bool conectar = true, bool loguear = true)
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
            clienteUdp = new UdpClient(host, puerto) { };
            if (timeoutLectura > 0)
            {
                clienteUdp.Client.ReceiveTimeout = timeoutLectura;
            }
        }

        public bool Conectado
        {
            get { return clienteUdp != null && clienteUdp.Client.Connected; }
        }

        public void ReConectar()
        {
            if (clienteUdp != null)
            {
                clienteUdp.Close();
            }
            Conectar();
        }

        public string EnviarComando(string comando, int posInicioRespuesta, int posFinRespuesta)
        {
            byte[] writeBuffer = Encoding.ASCII.GetBytes(comando);
            clienteUdp.Send(writeBuffer, writeBuffer.Length);

            var readBuffer = new byte[clienteUdp.Client.ReceiveBufferSize];
            clienteUdp.Client.Receive(readBuffer, readBuffer.Length, SocketFlags.None);

            if (comando == "P" && loguear)
            {
                log.Info("Comando - " + comando + " - Dispositivo: "+ host +":"+ puerto.ToString() +" Respuesta: '"+ (Encoding.ASCII.GetString(readBuffer.ToArray()).Replace("\r", "")) + "'");
            }
            
           return Encoding.ASCII.GetString(readBuffer, posInicioRespuesta, posFinRespuesta - posInicioRespuesta);
        }

        public void EnviarComando(string comando)
        {
            byte[] writeBuffer = Encoding.ASCII.GetBytes(comando);
            clienteUdp.Send(writeBuffer, writeBuffer.Length);
        }

        public void EnviarComando(List<byte> writeBuffer)
        {
            clienteUdp.Send(writeBuffer.ToArray(), writeBuffer.Count);
        }
        public void EnviarComando(byte[] writeBuffer)
        {
            clienteUdp.Send(writeBuffer, writeBuffer.Length);
        }
   
        public void Desconectar()
        {
            Dispose();
        }

        public void Dispose()
        {
            var disposable = clienteUdp as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
