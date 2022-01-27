using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace Molinos.Orquest.Web.Helpers
{

    public class JsonpResult : ActionResult
    {
        private readonly object obj;

        public JsonpResult(object obj)
        {
            this.obj = obj;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var serializer = new JavaScriptSerializer();
            var callbackname = context.HttpContext.Request["callback"];
            var jsonp = string.Format("{0}({1})", callbackname, serializer.Serialize(obj));
            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            response.Write(jsonp);
        }
    }
}
