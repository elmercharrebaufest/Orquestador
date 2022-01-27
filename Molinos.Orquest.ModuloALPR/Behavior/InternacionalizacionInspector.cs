using System.Globalization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;

namespace Molinos.Orquest.ModuloALPR.Behavior
{
    public class InternacionalizacionInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        private const string NombreHeader = "Culture";
        private const string Namespace = "http://scato.molinos.com";

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            var currentCulture = CultureInfo.CurrentUICulture.Name;
            request.Headers.Add(MessageHeader.CreateHeader(NombreHeader, Namespace, currentCulture));
            return currentCulture;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            string currentCulture = "";
            if (request.Headers.FindHeader(NombreHeader, Namespace) != -1)
            {
                currentCulture = request.Headers.GetHeader<string>(NombreHeader, Namespace);
                var ci = new CultureInfo(currentCulture);
                Thread.CurrentThread.CurrentUICulture = ci;
            }
            return currentCulture;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
