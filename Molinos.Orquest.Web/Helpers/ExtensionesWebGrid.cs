using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Web.Helpers
{
    public static class ExtensionesWebGrid
    {
        public static IHtmlString Grilla<TEntidad>(
            this HtmlHelper helper,
            ListaPaginada<TEntidad> items,
            Func<WebGrid, WebGridColumn[]> columnas)
        {
            var grid = new WebGrid(rowsPerPage: items.ItemsPorPagina,
                 sortDirectionFieldName: "dirOrden",
                 pageFieldName: "pagina",
                 sortFieldName: "ordenarPor");
            grid.Bind((IEnumerable<dynamic>)items.Items, autoSortAndPage: false, rowCount: items.ItemsTotales);

            return grid.GetHtml(
                tableStyle: "table table-striped table-hover",
                columns: columnas(grid),
                htmlAttributes: new { id = "grid" });
        }

        public static WebGridColumn Columna(this WebGrid grid, string columnName, string header,
                                           Func<dynamic, object> format = null, string style = null, bool canSort = true)
        {

            header += grid.SortColumn == columnName ? grid.SortDirection == SortDirection.Ascending ? "  ▲" : "  ▼" : " ▲▼";
            return grid.Column(columnName, header, format, style, canSort);
        }

        public static WebGridColumn ColumnaEstado(this WebGrid grid, string columnName, HtmlHelper html)
        {
            return grid.Column(columnName, columnName, f =>
                html.Raw(String.Format("<span class=\"badge estado-itc\" data-codigo=\"{0}\" data-toggle=\"tooltip\" title=\"{1}\">{2}</span>", f.Dispositivo.Codigo, Textos.ConsultandoEstado, Textos.ConsultandoEstado)),
                "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaEliminarModificarProbar(this WebGrid grid, HtmlHelper html, string controller, string style = "")
        {

            return grid.Column("EliminarModificar", "", f =>
                html.Raw(
                "<span>" +
                html.BotonLink(Textos.Modificar, "Modificar", controller, new { f.id }, style + " ajax-editar-link", "fa fa-edit", true).ToHtmlString() +
                html.BotonLink(Textos.Eliminar, "Eliminar", controller, new { f.id }, style + " ajax-borrar-link", "fa fa-trash", true).ToHtmlString() +
                html.BotonLink(Textos.Probar, "Probar", controller, new { f.id }, style, "fa fa-play", true).ToHtmlString() +
                "</span>"
                )
                , "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaEliminarModificar(this WebGrid grid, HtmlHelper html, string controller, string style = "")
        {

            return grid.Column("EliminarModificar", "", f =>
                html.Raw(
                "<span>" +
                html.BotonLink(Textos.Modificar, "Modificar", controller, new { f.id }, style + " ajax-editar-link", "fa fa-edit", true).ToHtmlString() +
                html.BotonLink(Textos.Eliminar, "Eliminar", controller, new { f.id }, style + " ajax-borrar-link", "fa fa-trash", true).ToHtmlString() +
                "</span>"
                )
                , "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaModificar(this WebGrid grid, HtmlHelper html, string controller, string style = "")
        {
            return grid.Column("editar", "", f => html.Raw(html.BotonLink(Textos.Modificar, "Modificar", controller, new { f.id }, style + " ajax-editar-link", "fa fa-edit", true).ToHtmlString()),
                                                        "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaEliminar(this WebGrid grid, HtmlHelper html, string controller, string style = "")
        {
            return grid.Column("Eliminar", "", f => html.Raw(html.BotonLink(Textos.Eliminar, "Eliminar", controller, new { f.id }, style + " ajax-borrar-link", "fa fa-trash", true).ToHtmlString()),
                                                        "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaSeleccionar(this WebGrid grid, HtmlHelper html, string controller, string style = "")
        {
            return grid.Column("Seleccionar", "Ver", f => html.Raw(html.BotonLink(Textos.Probar, "Probar", controller, new { f.id }, style, "fa fa-play", true).ToHtmlString()),
                                                        "editar-borrar-columna", false);
        }

        public static WebGridColumn ColumnaRadioBoton(this WebGrid grid, HtmlHelper html, string label, string name, bool required = false, string errorMessage = null)
        {
            return grid.Column("Seleccionar", "", f => html.Raw(html.RadioBoton(label, name, ((int)f.CentroId).ToString(CultureInfo.InvariantCulture), required, errorMessage).ToHtmlString()), null, false);
        }

        public static WebGridColumn ColumnaCerear(this WebGrid grid, HtmlHelper html, string texto)
        {

            return grid.Column("Cerear", "Cerear",
                               f => html.Raw(html.BotonId(texto, (int)f.Id, "btn botonCerear").ToHtmlString()),
                               "cereo-columna", false);
        }

        public static WebGridColumn ColumnaColor(this WebGrid grid, string columnName, string header,
                                           HtmlHelper html, string style = null, bool canSort = true)
        {

            header += grid.SortColumn == columnName ? grid.SortDirection == SortDirection.Ascending ? "  ▲" : "  ▼" : " ▲▼";
            return grid.Column(columnName, header, f => html.Raw(html.IconoColor((string)f.Color)), style, canSort);
        }
    }
}