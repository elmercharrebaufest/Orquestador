using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Molinos.Orquest.DriversImpl
{
    public static class ExtensionesTcpClient
    {
        public static TcpState GetState(this TcpClient tcpClient)
        {
            var foo = IPGlobalProperties.GetIPGlobalProperties()
              .GetActiveTcpConnections()
              .FirstOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint)
               && x.RemoteEndPoint.Equals(tcpClient.Client.RemoteEndPoint));
            return foo != null ? foo.State : TcpState.Unknown;
        }
    }
}
