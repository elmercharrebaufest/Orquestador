using Ninject.Extensions.Logging;
using System;
using System.Net.NetworkInformation;

namespace Molinos.Orquest.DriversImpl.Helpers
{
    public static class ConnectionHelper
    {
        public static ILogger Log { get; set; }

        public static bool IsConnected(TcpCommandClient cliente, bool pingOK, bool conectado)
        {
            return !cliente.Conectado || (pingOK && !conectado);
        }

        public static bool PingOK(string ip)
        {
            var pingOptions = new PingOptions(128, true);
            using (var ping = new Ping())
            {
                var buffer = new byte[32];

                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(ip, 3000, buffer, pingOptions);

                        if (pingReply != null && pingReply.Status == IPStatus.Success)
                        {
                            return true;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                }
            }
            return false;
        }
    }
}
