using Molinos.Orquest.DriversImpl.ServicioALPR;
using Ninject.Extensions.Logging;
using System;
using System.ServiceModel;

namespace Molinos.Orquest.DriversImpl.Helpers
{
    public static class ALPRConnectionHelper
    {
        private static ChannelFactory<IServicioALPR> _currentFactory;

        public static ChannelFactory<IServicioALPR> CurrentFactory
        {
            get
            {
                if (_currentFactory == null) _currentFactory = new ChannelFactory<IServicioALPR>("ServicioALPR");

                return _currentFactory;
            }
            private set
            {
                _currentFactory = value;
            }
        }

        public static T CreateChannel<T>(Func<IServicioALPR, T> accion, ILogger log)
        {
            IServicioALPR channel = null;
            try
            {
                channel = CurrentFactory.CreateChannel();
                return accion(channel);
            }
            catch
            {
                if (channel is ICommunicationObject comObject && comObject.State == CommunicationState.Faulted)
                {
                    log.Error("La conexión del ALPR entró en el estado faulted y fue abortada");
                    comObject.Abort();
                }
                throw;
            }
            finally
            {
                if (channel is ICommunicationObject comObject && comObject.State != CommunicationState.Faulted)
                {
                    try
                    {
                        comObject.Close();
                    }
                    catch
                    {
                        log.Error("La conexión del ALPR no pudo cerrarse correctamente y fue abortada");
                        comObject.Abort();
                    }
                }
            }
        }
    }
}