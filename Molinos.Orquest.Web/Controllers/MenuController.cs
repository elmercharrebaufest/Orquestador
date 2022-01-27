using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Web.Helpers;
using Molinos.Orquest.Web.Models;
using Ninject.Extensions.Logging;


namespace Molinos.Orquest.Web.Controllers
{
    public class MenuController : Controller
    {
        private ILogger log;

        public MenuController(ILogger log)
        {
            this.log = log;
        }

        public ActionResult Menu()
        {
            var rm = new ResourceManager(typeof(Textos));
            ViewBag.Idiomas = CultureInfo.GetCultures(CultureTypes.AllCultures).Select(x => x).Where(x => ResourceManagerExist(rm, x)).ToSelectList(x => x.LCID.ToString(CultureInfo.InvariantCulture), x => x.NativeName.Split('(')[0]);
            ViewBag.Idioma = CultureInfo.CurrentCulture.NativeName.Split('(')[0];
            return PartialView("_Menu");
        }

        private bool ResourceManagerExist(ResourceManager rm, CultureInfo c)
        {
            try
            {
                if (c.LCID != 127 && (rm.GetResourceSet(c, true, false) != null || c.LCID == 11274))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public ActionResult ChangeCurrentCulture(int lcid)
        {
            //
            // Change the current culture for this user.
            //
            var culture = CultureInfo.GetCultureInfo(lcid);
            SessionManager.CurrentCulture = culture;
            //
            // Cache the new current culture into the user HTTP session. 
            //
            var cookie = new CookieUsuario();
            cookie.ActualizarValor("CurrentCulture", lcid.ToString(CultureInfo.InvariantCulture));
            //
            // Redirect to the same page from where the request was made! 
            //
            return Redirect(Request.UrlReferrer.ToString());
        }

    }
}
