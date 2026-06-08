using Microsoft.AspNet.SignalR.Client;
using Ninject.Extensions.Logging;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace Molinos.Orquest.Web.ServicioHub
{
    public class HubClient : IDisposable
    {
        private IHubProxy proxy;
        private HubConnection connection;

        private readonly ILogger log;
        private readonly object connectionLock = new object();

        public HubClient(ILogger log)
        {
            this.log = log;

            try
            {
                lock (connectionLock)
                {
                    Connect();
                }
            }
            catch (Exception e)
            {
                log.Error(e, "HubClient: error al conectar al hub notificarLectura. Verificar 'UrlServicioNotificaciones' en config.");
            }
        }

        public Task Invoke(string method, params object[] args)
        {
            try
            {
                if (proxy == null)
                    throw new InvalidOperationException("El proxy SignalR es nulo; la conexión inicial falló.");

                return proxy.Invoke(method, args);
            }
            catch (InvalidOperationException e)
            {
                log.Warn(e, "HubClient: conexión perdida al invocar '{0}'. Intentando reconexión.", method);
                try
                {
                    Reconnect();
                    return proxy.Invoke(method, args);
                }
                catch (Exception ex)
                {
                    log.Error(ex, "HubClient: no se pudo reconectar al invocar '{0}'.", method);
                    throw;
                }
            }
        }

        private void Connect()
        {
            var url = ConfigurationManager.AppSettings["UrlServicioNotificaciones"];
            log.Debug("HubClient: conectando a '{0}' hub notificarLectura...", url);
            connection = new HubConnection(url);
            proxy = connection.CreateHubProxy("notificarLectura");
            connection.Start().Wait();
            log.Debug("HubClient: conexión establecida.");
        }

        private void Reconnect()
        {
            lock (connectionLock)
            {
                var state = connection?.State;
                if (state == ConnectionState.Disconnected || state == null)
                {
                    log.Debug("HubClient: reconectando...");
                    connection?.Dispose();
                    Connect();
                }
                else
                {
                    log.Debug("HubClient: reconexión omitida, estado actual={0}.", state);
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