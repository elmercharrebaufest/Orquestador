using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Molinos.Orquest.Web.ServicioHub
{
    public class HubClient : IDisposable
    {
        private IHubProxy proxy;
        private HubConnection connection;

        private readonly object connectionLock = new object();

        public HubClient()
        {

            try
            {
                lock (connectionLock)
                {
                    Connect();
                }
            }
            catch (Exception e)
            {
            }
        }

        public Task Invoke(string method, params object[] args)
        {
            try
            {
                return proxy.Invoke(method, args);
            }
            catch (InvalidOperationException e)
            {
                try
                {
                    Reconnect();
                    return proxy.Invoke(method, args);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }


        private void Connect()
        {
            connection = new HubConnection(ConfigurationManager.AppSettings["UrlServicioNotificaciones"]);
            proxy = connection.CreateHubProxy("notificarLectura");
            connection.Start().Wait();
        }

        private void Reconnect()
        {
            lock (connectionLock)
            {
                if (connection.State == ConnectionState.Disconnected)
                {
                    connection.Dispose();
                    Connect();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (connection != null)
                {
                    connection.Dispose();
                }
            }
        }
    }
}