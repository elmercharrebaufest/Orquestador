using System.Web.Mvc;

namespace Molinos.Orquest.Web
{
    public class AjaxEditSuccessResult : ContentResult
    {
        public const string SuccessValue = "ajax-edit-success";

        public AjaxEditSuccessResult()
        {
            Content = SuccessValue;
        }
    }
}