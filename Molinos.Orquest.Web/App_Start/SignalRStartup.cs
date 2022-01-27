using System.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Molinos.Orquest.Web.App_Start;
using Owin;

[assembly: OwinStartup(typeof(SignalRStartup))]
namespace Molinos.Orquest.Web.App_Start
{
    public class SignalRStartup
    {
        public void Configuration(IAppBuilder app)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["OrquestadorDb"].ConnectionString;
            GlobalHost.DependencyResolver.UseSqlServer(connectionString);
            app.MapSignalR();
        }
    }
}