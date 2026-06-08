# Referencia: Vistas Razor

## Estructura de archivos

```
Views\{NombreDominio}\
    Index.cshtml          ← full page, carga Layout, incluye Listar como partial
    Listar.cshtml         ← tabla paginada (ajax refresh)
    Crear.cshtml          ← full page, incluye _CrearModificar
    Modificar.cshtml      ← full page, incluye _CrearModificar
    _CrearModificar.cshtml ← partial, formulario completo
```

---

## Index.cshtml

```cshtml
@model string
@{
    ViewBag.Title = Textos.{NombreDominio}_Titulo;
}

<div class="{pref}-page">
    <div class="{pref}-header">
        <h1 class="{pref}-header__title">@Textos.{NombreDominio}_Titulo</h1>
        <div class="{pref}-header__actions">
            @Html.BotonLink(Textos.Crear, "Crear", "{NombreDominio}", null, "fa fa-plus", "")
        </div>
    </div>
    @Html.Partial("Listar", Model)
</div>
```

---

## Listar.cshtml

```cshtml
@using Molinos.Orquest.Dominio.Recursos
@model string
@{
    var items = ViewBag.Items as Molinos.Orquest.Dominio.Consultas.ListaPaginada<Molinos.Orquest.Dominio.Entidades.{EntidadDominio}>;
}

@using (Html.BeginForm("Index", "{NombreDominio}", FormMethod.Get, new { id = "form-filtro" }))
{
    <div class="form-inline mb-3">
        @Html.TextBox("filtro", Model, new { @class = "form-control mr-2", placeholder = Textos.Filtrar })
        <button type="submit" class="btn btn-default">@Textos.Filtrar</button>
    </div>
}

<table class="table table-hover">
    <thead>
        <tr>
            @Html.EncabezadoTabla("Nombre",  "Nombre",  Model)
            @Html.EncabezadoTabla("Codigo",  "Codigo",  Model)
            @Html.EncabezadoTabla("Activo",  "Activo",  Model)
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var c in items)
        {
            <tr>
                <td>@c.Nombre</td>
                <td>@c.Codigo</td>
                <td>@(c.Activo ? "Si" : "No")</td>
                <td>
                    @Html.BotonLink(Textos.Modificar, "Modificar", "{NombreDominio}", new { id = c.Id }, "fa fa-edit", "")
                    @Html.BotonLink(Textos.Eliminar,  "Eliminar",  "{NombreDominio}", new { id = c.Id }, "fa fa-trash", " ajax-borrar-link")
                </td>
            </tr>
        }
    </tbody>
</table>

@Html.Paginacion(items, Model)

<script>
    $(function () {
        if (!@Html.IsPermiso(PermisosOrquestador.{NombrePermiso}Editar)) {
            $(".ajax-borrar-link").hide();
        }
    });
</script>
```

### Notas críticas de Listar

- **Modificar**: `BotonLink` 5° param = cssClass del `<a>`. Usar `""` (vacío) para full-page navigation. Usar `" ajax-editar-link"` solo si abre modal Ajax.
- **Eliminar**: siempre `" ajax-borrar-link"`.
- **Activo**: siempre `"Si"` / `"No"` en ASCII. Nunca `"Sí"` ni `"—"` (caracteres no-ASCII corrompen según encoding del servidor).
- El `BotonLink` helper recibe: `(texto, accion, controller, routeValues, iconClass, cssClass)`.

---

## Crear.cshtml

```cshtml
@using Molinos.Orquest.Dominio.Recursos
@model Molinos.Orquest.Web.Models.{NombreDominio}Model
@{
    ViewBag.Title = Textos.{NombreDominio}_Crear;
}

<div class="{pref}-header">
    <h1 class="{pref}-header__title">@Textos.{NombreDominio}_Crear</h1>
    <div class="{pref}-header__actions">
        @using (Html.BeginForm("Crear", "{NombreDominio}", FormMethod.Post))
        {
            @Html.AntiForgeryToken()
            <button type="submit" class="btn btn-primary {pref}-header__save">
                <span class="fa fa-save"></span> @Textos.Guardar
            </button>
            @Html.ActionLink(Textos.Cancelar, "Index", "{NombreDominio}", null, new { @class = "btn btn-default" })
            @Html.Partial("_CrearModificar", Model)
        }
    </div>
</div>
```

---

## Modificar.cshtml

Idéntico a Crear pero con acción `"Modificar"` y título `Textos.{NombreDominio}_Modificar`.
Incluir `@Html.HiddenFor(m => m.Id)` dentro del form.

---

## _CrearModificar.cshtml

```cshtml
@using Newtonsoft.Json
@using Molinos.Orquest.Dominio.Recursos
@model Molinos.Orquest.Web.Models.{NombreDominio}Model

@{
    var jsonSettings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml };
    // Serializar datos para JS si aplica
}

<link href="~/Styles/variables.css" rel="stylesheet" />
<link href="~/Styles/{nombreDominioCamel}.css" rel="stylesheet" />

<div class="{pref}-form" id="{pref}-form-container">

    <div class="{pref}-card">
        <div class="{pref}-card__header">
            <span class="{pref}-card__icon"><span class="fa fa-info-circle"></span></span>
            <h2 class="{pref}-card__title">@Textos.{NombreDominio}_InformacionGeneral</h2>
        </div>
        <div class="{pref}-card__body">
            <div class="{pref}-form-row">
                <div class="{pref}-form-group">
                    @Html.LabelFor(m => m.Nombre, new { @class = "{pref}-form-group__label" })
                    @Html.TextBoxFor(m => m.Nombre, new { @class = "{pref}-form-group__input" })
                    @Html.ValidationMessageFor(m => m.Nombre, null, new { @class = "{pref}-form-group__error" })
                </div>
            </div>

            {{/* switch Activo */}}
            <div class="{pref}-switch">
                @Html.CheckBoxFor(m => m.Activo, new { @class = "{pref}-switch__input", id = "{pref}-activo" })
                <label class="{pref}-switch__track" id="{pref}-activo-track">
                    <span class="{pref}-switch__thumb"></span>
                </label>
                @Html.LabelFor(m => m.Activo, new { @class = "{pref}-switch__label" })
            </div>
        </div>
    </div>

</div>

{{/* Inyección de datos para JS */}}
<script>
    window.{camel}SomeData = @Html.Raw(JsonConvert.SerializeObject(someData, jsonSettings));
</script>

<script src="~/Scripts/{nombreDominioCamel}.js"></script>
```

### Notas críticas de _CrearModificar

- **Switch Activo**: La label del track NO lleva `for="..."`. El JS maneja el toggle explícitamente (`input.checked = !input.checked`). Esto evita el double-fire del browser cuando MVC inserta un hidden input entre el checkbox y la label.
- **CSS selector**: Usar `~` (hermano general) en lugar de `+` (adyacente) para `.{pref}-switch__input:checked ~ .{pref}-switch__track` porque MVC inserta `<input type="hidden">` entre el checkbox y la label.
- **Datos al cliente**: siempre via `<script>window.{camel}* = @Html.Raw(...)</script>`, nunca via `data-*` atributos (los JSON con comillas dobles rompen el atributo).
- **`StringEscapeHandling.EscapeHtml`**: obligatorio en todo `JsonConvert.SerializeObject` inyectado en Razor.
