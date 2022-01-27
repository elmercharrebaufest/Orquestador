using log4net;
using Molinos.Orquest.ModuloALPR.App_Start;

namespace Molinos.Orquest.ModuloALPR
{
    public class Global : System.Web.HttpApplication
    {

        protected void Application_Start()
        {
            Log4NetConfig.Configure(Server);
        }

        protected void Application_Error()
        {
            var ex = Server.GetLastError();
            var logger = LogManager.GetLogger(GetType());
            logger.Error("Excepción no manejada: ", ex);
        }
    }
}