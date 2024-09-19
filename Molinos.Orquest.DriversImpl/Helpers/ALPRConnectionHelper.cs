using Molinos.Orquest.DriversImpl.ServicioALPR;
using System;
using System.ServiceModel;

namespace Molinos.Orquest.DriversImpl.Helpers
{
    public static class ALPRConnectionHelper
    {
        public static T CreateChannel<T>(Func<IServicioALPR, T> accion)
        {
            var channelFactory = new ChannelFactory<IServicioALPR>("ServicioALPR");

            IServicioALPR channel = null;
            try
            {
                channel = channelFactory.CreateChannel();
                return accion(channel);
            }
            catch
            {
                if (channel is ICommunicationObject comObject && comObject.State == CommunicationState.Faulted)
                {
                    comObject.Abort();
                }
                throw;
            }
            finally
            {
                if (channel is ICommunicationObject comObject && comObject.State != CommunicationState.Faulted)
                {
                    comObject.Close();
                }
            }
        }
    }
}
