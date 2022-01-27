using System.ServiceModel;

namespace Molinos.Orquest.Servicios.Impl
{
    public static class ChannelExtensions
    {       
        public static void DisposeChannelProxy(this object channelProxy)
        {
            var comObject = channelProxy as ICommunicationObject;
            if (comObject != null)
            {
                comObject.CloseCommunicationObject();
            }
        }
        
        public static void CloseCommunicationObject(this ICommunicationObject comObject)
        {
            try
            {
                if (comObject.State != CommunicationState.Faulted)
                {
                    comObject.Close();
                }
            }
            catch
            {
                try
                {
                    comObject.Abort();
                }
                catch
                {
                    // Si ocurre un errr en este punto no hay nada más para hacer
                }
            }
        }
    }
}
