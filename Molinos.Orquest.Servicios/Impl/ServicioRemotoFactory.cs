using System;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Molinos.Orquest.Servicios.Impl
{
    public sealed class ServicioRemotoFactory : IServicioRemotoFactory, IDisposable
    {
        private readonly ConcurrentDictionary<string, ChannelFactory<IServicioOrquestador>> factoriesOrquestadores = new ConcurrentDictionary<string, ChannelFactory<IServicioOrquestador>>();
        private readonly ConcurrentDictionary<string, ChannelFactory<IServicioSuscriptor>> factoriesSuscriptores = new ConcurrentDictionary<string, ChannelFactory<IServicioSuscriptor>>();

        public ClienteServicio<IServicioOrquestador> CrearServicioOrquestador(string rutaServicio)
        {
            var canal = factoriesOrquestadores.GetOrAdd(rutaServicio, CrearChannelFactoryOrquestador);
            return new ClienteServicio<IServicioOrquestador>(canal.CreateChannel());
        }

        private ChannelFactory<IServicioOrquestador> CrearChannelFactoryOrquestador(string rutaServicio)
        {
            var binding = rutaServicio.StartsWith("net.tcp") ? (Binding) new NetTcpBinding("NetTcpBinding") : new BasicHttpBinding("CommonBinding");
            return new ChannelFactory<IServicioOrquestador>(binding, new EndpointAddress(rutaServicio));
        }

        public ClienteServicio<IServicioSuscriptor> CrearServicioSuscriptor(string rutaServicio)
        {
            var canal = factoriesSuscriptores.GetOrAdd(rutaServicio, CrearChannelFactorySuscriptor);
            return new ClienteServicio<IServicioSuscriptor>(canal.CreateChannel());
        }

        private ChannelFactory<IServicioSuscriptor> CrearChannelFactorySuscriptor(string rutaServicio)
        {
            return new ChannelFactory<IServicioSuscriptor>(new BasicHttpBinding("SuscriptorBinding"),
                                                           new EndpointAddress(rutaServicio));
        }

        public void Dispose()
        {
            foreach (var factory in factoriesSuscriptores)
            {
               factory.Value.CloseCommunicationObject();
            }
            foreach (var factory in factoriesOrquestadores)
            {
                factory.Value.CloseCommunicationObject();
            }
        }
    }

    public sealed class ClienteServicio<TServicio> : IDisposable
    {
        public TServicio Servicio { get; private set; }

        public ClienteServicio(TServicio servicio)
        {
            Servicio = servicio;
        }

        public void Dispose()
        {
            Servicio.DisposeChannelProxy();
        }
    }
}
