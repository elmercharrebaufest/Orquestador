---
name: abm-mvc
description: >
  Genera un ABM (Alta-Baja-Modificación) completo en ASP.NET MVC 5 para el proyecto Orquestador/Molinos.Orquest.Web.
  Usa cuando el usuario pida un nuevo ABM, CRUD, administración, listado con edición, o mantenimiento de entidades.
  Incluye: model, AutoMapper profile, controller, vistas Razor, CSS BEM, JS vanilla, recursos i18n, permiso, menú, tests unitarios NUnit, tests E2E Playwright, registro en csproj, compilación y verificación.
argument-hint: 'Nombre del dominio (ej: ConfigBarrera, Sensor, Rol) o descripción de la entidad a administrar'
---

# ABM MVC — Guía Completa

## Pre-flight: recopilar información

Antes de escribir código, usar `vscode_askQuestions` para recopilar todo lo necesario en un único bloque.
Si algún dato ya fue proporcionado en el mensaje, usarlo directamente sin volver a preguntar.

### Información requerida

| # | Dato | Pregunta si falta |
|---|------|-------------------|
| 1 | **NombreDominio** | Nombre de la entidad de dominio (PascalCase, ej: `ConfigBarrera`) |
| 2 | **NombrePermiso** | Nombre del permiso en `PermisosOrquestador` (ej: `ConfigBarrera`). Si no se conoce, sugerir igual al dominio |
| 3 | **UbicacionMenu** | En qué submenú del `_Menu.cshtml` aparecerá (ej: "Grupos de dispositivos", "Administración", nivel raíz) |
| 4 | **Campos** | Campos del formulario con tipo y validaciones. Si no se dan, pedir imagen/descripción de la pantalla |
| 5 | **EntidadBase** | Si la entidad ya existe en `Molinos.Orquest.Dominio` o hay que crearla |

> Si se adjunta imagen de la pantalla, inferir los campos desde ella y confirmar antes de continuar.

---

## Convención de nombres

Todos los artefactos usan `{NombreDominio}` como prefijo/sufijo para facilitar búsquedas:

| Artefacto | Nombre |
|-----------|--------|
| Controller | `{NombreDominio}Controller.cs` |
| Model | `{NombreDominio}Model.cs` |
| Mapping profile | `{NombreDominio}MappingProfile.cs` |
| CSS | `{nombrDominioCamel}.css` |
| JS | `{nombreDominioCamel}.js` |
| Variables CSS | `variables.css` (compartido) |
| Vistas | `Views\{NombreDominio}\` |
| Test controller | `{NombreDominio}ControllerTest.cs` |
| Test E2E | `{NombreDominio}.spec.js` |
| Textos recurso | prefijo `{NombreDominio}_` |

---

## Pasos de implementación

### Paso 1 — Recursos i18n

Ver [./references/resources.md](./references/resources.md)

- Agregar entradas en `Textos.resx` (ES) y `Textos.en.resx` (EN)
- Actualizar `Textos.Designer.cs` con la propiedad estática correspondiente
- Claves mínimas: `_Titulo`, `_Administrar`, `_Crear`, `_Modificar` y una por campo del formulario

### Paso 2 — Modelo y Mapping

Ver [./references/model-mapping.md](./references/model-mapping.md)

- Crear `Models\{NombreDominio}Model.cs` con data annotations
- Crear `Conversiones\Impl\Perfiles\{NombreDominio}MappingProfile.cs`
- Si hay sub-entidades de colección (como cámaras), usar clases anidadas en el Model con `{Id, ...}` PascalCase español

### Paso 3 — Controller

Ver [./references/controller.md](./references/controller.md)

- Constructor: `(IRepositorioFactory, IConversor, IServicioOrquestador, ILogger)`
- Acciones: `Index`, `Listar` (AjaxOnly), `Crear` (GET/POST), `Modificar` (GET/POST), `Eliminar` (POST)
- Método `PopularDropdowns()` para `ViewBag`
- Usar `repositorio.Listar<T>()`, `repositorio.Obtener<T>(id)`, `repositorio.Agregar`, `repositorio.Remover`, `repositorio.GuardarCambios()`
- Para colecciones de navegación en Modificar: nunca reasignar (EF proxy); usar `.Clear()` / `.Add()` o iterar con `Remover` + `Add`

### Paso 4 — Vistas Razor

Ver [./references/views.md](./references/views.md)

- `Index.cshtml`: Layout completo, `@Html.Partial("Listar", Model)`
- `Listar.cshtml`: Tabla paginada, botón Crear como link, Modificar sin clase ajax (full page), Eliminar con clase `ajax-borrar-link`
- `Modificar.cshtml` y `Crear.cshtml`: Full-page (con Layout), renderizan `@Html.Partial("_CrearModificar", Model)`
- `_CrearModificar.cshtml`: Partial sin Layout. Incluye inyección de datos via `<script>window.{camel}* = @Html.Raw(...)</script>` con `StringEscapeHandling.EscapeHtml`
- Columna Activo en Listar: `c.Activo ? "Si" : "No"` (ASCII)
- Switch para campo Activo: usar patrón `civ-switch` con `id` en el input y JS que lo controle (no `for` en la track label)

### Paso 5 — CSS (BEM)

Ver [./references/css-js.md](./references/css-js.md)

- Incluir `variables.css` (`:root` con custom properties) y `{nombreDominioCamel}.css`
- BEM estricto: `.{bloque}`, `.{bloque}__{elemento}`, `.{bloque}--{modificador}`
- Usar prefijo corto del dominio (ej: `civ-` para ConfigIdentificacionVehicular, `bar-` para Barrera)
- No usar `+` como selector de hermano adyacente si MVC puede insertar hidden inputs entre elementos — usar `~`
- No usar `:root` en el CSS específico del ABM; solo en `variables.css`

### Paso 6 — JS (Vanilla)

Ver [./references/css-js.md](./references/css-js.md)

- IIFE envolvente: `(function () { 'use strict'; ... })()`
- `document.addEventListener('DOMContentLoaded', ...)` como punto de entrada
- `const` / `let`; nunca `var`
- **No usar guards `if (element)` en `document.getElementById`** — asumir que el elemento existe
- Datos del servidor: `window.{prefijo}*` inyectados desde `<script>` en la vista (no `data-*` atributos)
- Nombres de propiedades de objetos: PascalCase español (`{Id, Nombre, Ip}`) — coherente con JSON del servidor

### Paso 7 — Permiso

- Agregar la constante en `PermisosOrquestador.cs` en `Molinos.Orquest.Dominio\Seguridad`
- Decorar el controller con `[Autorizacion(PermisosOrquestador.{NombrePermiso})]`
- **Solicitar al usuario que haga Publish del proyecto `Molinos.Orquest.Database`** para ejecutar la migración de datos de permiso en BD local antes de continuar con la compilación

### Paso 8 — Menú

- Agregar entrada en `Views\Shared\_Menu.cshtml` dentro del submenú indicado en pre-flight
- Usar `@if (PermisosHelper.Is(PermisosOrquestador.{NombrePermiso}))` y `Textos.{NombreDominio}_Administrar`

### Paso 9 — Registro en csproj

Ver [./references/csproj-menu-db.md](./references/csproj-menu-db.md)

Verificar que cada artefacto nuevo esté registrado en `Molinos.Orquest.Web.csproj`:
- `<Compile Include="..." />` para `.cs`
- `<Content Include="..." />` para `.cshtml`, `.css`, `.js`

Si falta alguno, agregarlo en la sección correspondiente (ordenado alfabéticamente o junto a archivos del mismo tipo).

### Paso 10 — Database publish (antes de compilar)

**Antes de ejecutar el build, informar al usuario:**

> "Por favor, haz Publish del proyecto `Molinos.Orquest.Database` apuntando a tu instancia local para aplicar las migraciones (nuevas tablas y registros de permiso). Cuando esté listo, continúa."

Esperar confirmación antes de ejecutar MSBuild.

### Paso 11 — Compilar y verificar

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  "c:\Repositorios\MOA\Orquestador\Molinos.Orquestador.sln" `
  /p:Configuration=Debug /nologo /v:m 2>&1 | Select-String " error "
```

Sin resultados = build limpio. Si hay errores, corregir antes de continuar.

### Paso 12 — Unit tests (NUnit)

Ver [./references/tests.md](./references/tests.md)

- Crear `Molinos.Orquest.Test\Controllers\{NombreDominio}ControllerTest.cs`
- Patrón: `[TestFixture]`, `[SetUp]` con Mocks, un `[Test]` por acción del controller
- Usar `Mock<IRepositorioFactory>`, `Mock<IRepositorio>`, `Mock<IConversor>`, `Mock<IServicioOrquestador>`
- Verificar también que `ViewBag` se popule correctamente en GET y en redirección con errores de validación

### Paso 13 — E2E tests (Playwright)

Ver [./references/tests.md](./references/tests.md)

- Crear `Molinos.Orquest.E2ETests\tests\{NombreDominio}.spec.js`
- `test.describe` por sección: Listado, Formulario Crear, Formulario Modificar, Eliminar
- Usar `BASE_URL` desde `.env`
- Validar: título visible, campos del formulario presentes, validaciones de requerido, navegación entre páginas
- Ejecutar: `npx playwright test tests/{NombreDominio}.spec.js --reporter=line`

---

## Checklist final

- [ ] Recursos en `Textos.resx`, `Textos.en.resx` y `Textos.Designer.cs`
- [ ] `{NombreDominio}Model.cs` con data annotations
- [ ] `{NombreDominio}MappingProfile.cs`
- [ ] `{NombreDominio}Controller.cs` con las 5 acciones
- [ ] `Views\{NombreDominio}\` con Index, Listar, Crear, Modificar, _CrearModificar
- [ ] `{nombreDominioCamel}.css` con BEM y `variables.css` incluido
- [ ] `{nombreDominioCamel}.js` con IIFE, DOMContentLoaded, sin guards en DOM
- [ ] Permiso agregado en `PermisosOrquestador.cs`
- [ ] Entrada en `_Menu.cshtml` con el permiso correcto
- [ ] Todos los archivos registrados en `Molinos.Orquest.Web.csproj`
- [ ] Usuario confirmó Publish del Database
- [ ] Build sin errores
- [ ] `{NombreDominio}ControllerTest.cs` creado y compilando
- [ ] `{NombreDominio}.spec.js` creado y corriendo
