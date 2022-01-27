using System.Globalization;
using System.Threading;
using System.Web.SessionState;

namespace Molinos.Orquest.Web
{
    public class SessionManager
    {
        protected HttpSessionState session;

        public SessionManager(HttpSessionState httpSessionState)
        {
            session = httpSessionState;
        }

        public static CultureInfo CurrentCulture
        {
            set
            {
                Thread.CurrentThread.CurrentUICulture = value;
                Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture;
            }
        }
    }
}