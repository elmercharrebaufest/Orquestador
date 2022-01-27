using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.DriversImpl
{
    public sealed class TcpCommandListener : IDisposable
    {
        private TcpListener listenerTcp;
        private readonly string host;
        private readonly int puerto;
        private readonly int tamBuffer;
        private readonly int timeoutLectura;
        private readonly ILogger log;
        private readonly bool loguear;
        private TcpClient connectedTcpClient;
        public TcpCommandListener(string host, int puerto, int tamBuffer, int timeoutLectura, ILogger log, bool conectar = true, bool loguear = true)
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
            listenerTcp = new TcpListener(IPAddress.Parse(host), puerto);
            listenerTcp.Start();
        }

        public string RecibirMensaje()
        {
            Byte[] bytes = new Byte[tamBuffer];
            var retorno = string.Empty;
            using (connectedTcpClient = listenerTcp.AcceptTcpClient())
            {
                // Get a stream object for reading 					
                using (NetworkStream stream = connectedTcpClient.GetStream())
                {
                    int length = stream.Read(bytes, 0, bytes.Length);
                    var incommingData = new byte[length];
                    Array.Copy(bytes, 0, incommingData, 0, length);
                    retorno = Encoding.ASCII.GetString(incommingData);
                }
            }
            return retorno;
        }

        public bool Conectado
        {
            get { return true; }
        }

        public void ReConectar()
        {
            if (listenerTcp != null)
            {
                Dispose();
            }
            Conectar();
        }

        public void Desconectar()
        {
            Dispose();
        }

        public void Dispose()
        {
            listenerTcp.Stop();
            var disposable = connectedTcpClient as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
