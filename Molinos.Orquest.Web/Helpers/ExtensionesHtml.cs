using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Molinos.Orquest.Web.Helpers
{
    public static class ExtensionesHtml
    {
        public static MvcHtmlString BotonLink(this HtmlHelper helper, string textoBoton, string action, string controller, Object parameters, string style = "btn", string icono = "", bool ocultarTexto = false)
        {
            var urlHelper = new UrlHelper(helper.ViewContext.RequestContext);
            var url = urlHelper.Action(action, controller, parameters);
            var html = new StringBuilder();
            html.Append("<a href=\"");
            html.Append(url + "\"");
            html.Append(" title = \"" + textoBoton + "\"");
            html.Append(" class = \"" + style + "\"");
            html.Append(">");
            html.Append("<i class=\"" + icono + "\"></i>");
            if (!ocultarTexto)
            {
                html.Append(textoBoton);
            }
            html.Append("</a>");
            return MvcHtmlString.Create(html.ToString());
        }

        public static MvcHtmlString BotonId(this HtmlHelper helper, string textoBoton, int id, string style = "btn", string icono = "", bool ocultarTexto = false)
        {
            var html = new StringBuilder();
            html.Append("<a href=\"#\"");
            html.Append(" title = \"" + textoBoton + "\"");
            html.Append(" data-boton-cerear-id = \"" + id + "\"");
            html.Append(" class = \"" + style + "\"");
            html.Append(">");
            html.Append("<i class=\"" + icono + "\"></i>");
            if (!ocultarTexto)
            {
                html.Append(textoBoton);
            }
            html.Append("</a>");
            return MvcHtmlString.Create(html.ToString());
        }

        public static MvcHtmlString RadioBoton(this HtmlHelper helper, string label, string name, string value, bool required = false, string errorMessage = null)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"control-group\">");
            html.AppendLine("<div class=\"controls\">");
            html.AppendLine("<label class=\"radio inline\">");
            html.AppendLine("<input name=\"" + name + "\" id=\"" + name + "\" type=\"radio\" data-val-required=\"" + errorMessage + "\" data-val=\"" + required.ToString().ToLower() + "\" value=\"" + value + "\"/>");
            html.AppendLine("</label>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            return MvcHtmlString.Create(html.ToString());
        }

        public static MvcHtmlString IconoColor(this HtmlHelper helper, string color)
        {
            var html = new StringBuilder();
            html.AppendLine("<div class=\"iconoColor\" style=\"background-color:");
            html.AppendLine(color);
            html.AppendLine(";\">&nbsp;</div>");
            html.AppendLine("</div>");
            return MvcHtmlString.Create(html.ToString());
        }

        public static MvcHtmlString Breadcrumb(this HtmlHelper helper, string paginaActual, params string[] titulosPath)
        {
            var html = new StringBuilder();
            html.Append("<ul class=\"breadcrumb\">");

            foreach (var tituloPath in titulosPath)
            {
                html.AppendFormat("<li>{0} <span class=\"divider\">\\</span></li>", tituloPath);
            }
            html.AppendFormat("<li>{0}</li>", paginaActual);
            html.Append("</ul>");
            return new MvcHtmlString(html.ToString());
        }
        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, bool displayCode = false) where TModel : class
        {
            return htmlHelper.DropDownListFor(expression, GetSelectListItems(htmlHelper, expression, displayCode), htmlAttributes);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, string optionLabel, bool displayCode = false) where TModel : class
        {
            return htmlHelper.DropDownListFor(expression, GetSelectListItems(htmlHelper, expression, displayCode), optionLabel);
        }

        public static MvcHtmlString EnumDropDownListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, string name, Expression<Func<TModel, TProperty>> expression, object htmlAttributes = null, bool displayCode = false) where TModel : class
        {
            return htmlHelper.DropDownList(name, GetSelectListItems(htmlHelper, expression, displayCode), htmlAttributes);
        }

        private static IEnumerable<SelectListItem> GetSelectListItems<TModel, TProperty>(HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, bool displayCode) where TModel : class
        {
            var type = typeof(TProperty);
            if (Nullable.GetUnderlyingType(type) != null)
            {
                type = Nullable.GetUnderlyingType(type);
            }
            var values = Enum.GetValues(type);
            var model = htmlHelper.ViewData.Model;
            var propertyValue = (model != null)
                                     ? Convert.ToString(expression.Compile().Invoke(model))
                                     : values.GetValue(0).ToString();

            return values.Cast<Enum>()
                .Select(value => new SelectListItem
                {
                    Value = Convert.ToString(value),
                    Text = displayCode ? Convert.ToString(value) + " - " + DisplayText(value) : DisplayText(value),
                    Selected = Convert.ToString(value) == propertyValue
                }).ToList();
        }

        public static string DisplayText(Enum enumValue)
        {
            var displayText = enumValue.ToString();

            var field = enumValue.GetType().GetField(displayText);

            if (field == null)
            {
                return string.Empty;
            }

            var attributes = (DisplayAttribute[])field.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (attributes.Length > 0)
            {
                var resourceType = attributes[0].ResourceType;
                var property = attributes[0].Name;
                displayText = resourceType.GetProperty(property).GetValue(resourceType, null).ToString();
            }
            return displayText;
        }
    }
}