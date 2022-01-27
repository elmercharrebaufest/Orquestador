using System;
using System.IO;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;
using System.Xml.Linq;

namespace Molinos.Orquest.ModuloALPR.Behavior
{
    public class SimpleMessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        #region IClientMessageInspector Members

        public void AfterReceiveReply(ref Message reply, object correlationState)
        { }

        public object BeforeSendRequest(ref Message request, System.ServiceModel.IClientChannel channel)
        {

            //modify the request send from client(only customize message body)
            request = TransformMessage(request);

            return null;
        }
        
        //only read and modify the Message Body part
        private Message TransformMessage(Message oldMessage)
        {
            Message newMessage;

            //load the old message into XML
            MessageBuffer msgbuf = oldMessage.CreateBufferedCopy(int.MaxValue);

            Message tmpMessage = msgbuf.CreateMessage();
            XmlDictionaryReader xdr = tmpMessage.GetReaderAtBodyContents();

            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(xdr);
            xdr.Close();

            XmlElement node = xdoc.DocumentElement;
            node.Prefix = "pod";

            var loaded = new XDocument();
            if (xdoc.DocumentElement.NamespaceURI != String.Empty)
            {
                var xml = xdoc.OuterXml.Replace("xmlns=\"http://pod.waybillmanagement.ws.industrysystem.com.ar/\"", "");
                loaded = XDocument.Parse(xml);
            }

            MemoryStream ms = new MemoryStream();
            XmlWriter xw = XmlWriter.Create(ms);
            loaded.Save(xw);
            xw.Flush();
            xw.Close();

            ms.Position = 0;
            XmlReader xr = XmlReader.Create(ms);


            //create new message from modified XML document
            newMessage = Message.CreateMessage(oldMessage.Version, null, xr);
            newMessage.Headers.CopyHeadersFrom(oldMessage);
            newMessage.Properties.CopyProperties(oldMessage.Properties);

            return newMessage;
        }

        #endregion



        #region IDispatchMessageInspector Members

        public object AfterReceiveRequest(ref Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext)
        {
            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        { }
        #endregion
    }
}
