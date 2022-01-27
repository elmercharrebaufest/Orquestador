using System.Web.Mvc;
using Molinos.Orquest.Web.Atributos;

namespace Molinos.Orquest.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AvoidCacheFilterAttribute());
        }
    }
}