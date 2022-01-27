using System.IO;
using System.Web;

namespace Molinos.Orquest.Web.App_Start
{
    public static class Log4NetConfig
    {
        public static void Configure(HttpServerUtility server)
        {
            var fileInfo = new FileInfo(server.MapPath("~/log4net.config"));
            log4net.Config.XmlConfigurator.ConfigureAndWatch(fileInfo);
        }
    }
}