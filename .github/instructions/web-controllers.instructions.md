---
applyTo: "**/Molinos.Orquest.Web/Controllers/**/*.cs"
---

# Reglas de la capa Web — Controllers

## Principio fundamental
Los controllers son la interfaz entre el usuario y el sistema. Su responsabilidad es **recibir la request, delegar al servicio u orquestador, y devolver la vista o resultado**. No contienen lógica de negocio ni acceso directo a drivers.

## Herencia obligatoria

Todo controller hereda de `BaseController`:

```csharp
public class NuevoTipoController : BaseController
{
    public NuevoTipoController(
        IRepositorioFactory repositorio,
        IServicioOrquestador servicio,
        ILogger log) : base(repositorio, servicio, log)
    {
    }
}
```

`BaseController` provee: `repositorio`, `servicio`, `log`, internacionalización, y los métodos helper `SetearVistaConfiguracion()` y `ValidacionesDeNegocio()`.

## Un controller por tipo de dispositivo

- Nombre: `{TipoDispositivo}Controller` (ej.: `BarreraController`, `CamaraController`).
- Archivo: `Controllers/{TipoDispositivo}Controller.cs`.
- Cada controller gestiona el CRUD de configuración de **un único tipo de dispositivo**.

## Autorización

Todo controller de dispositivo lleva el atributo de autorización:

```csharp
[Autorizacion(PermisosOrquestador.NuevoTipo)]
public class NuevoTipoController : BaseController
```

Agregar el permiso correspondiente en `PermisosOrquestador` antes de crear el controller.

## Patrón de acciones CRUD

Seguir el patrón establecido en controllers existentes (ej.: `BarreraController`):

```csharp
// GET lista (retorna la vista completa)
public ActionResult Index(string filtro, int pagina = 1, string ordenarPor = "Id", DirOrden dirOrden = DirOrden.Asc)

// GET lista parcial para AJAX
[AjaxOnly]
[ActionName("Index")]
public ActionResult Listar(string filtro, int pagina = 1, ...)

// GET formulario de creación
public ActionResult Crear()

// POST creación
[HttpPost]
public ActionResult Crear(ConfigNuevoTipo model)

// GET formulario de edición
public ActionResult Editar(int id)

// POST edición
[HttpPost]
public ActionResult Editar(ConfigNuevoTipo model)

// POST eliminación
[HttpPost]
public ActionResult Eliminar(int id)
```

## Drivers disponibles en vistas de configuración

Usar `IDriverFactory` para obtener los drivers disponibles del tipo correcto e inyectarlo por constructor:

```csharp
private readonly IEnumerable<string> drivers;

public NuevoTipoController(IRepositorioFactory repositorio, IDriverFactory driverFactory,
    IServicioOrquestador servicio, ILogger log) : base(repositorio, servicio, log)
{
    drivers = driverFactory.DriversDisponibles<IDriverNuevoTipo>();
}
```

Luego en las acciones `Crear()` y `Editar()`:
```csharp
SetearVistaConfiguracion(drivers);
```

## Uso de `repositorio` en controllers

El `repositorio` del `BaseController` es adecuado para:
- Lookups de configuración de dispositivos (CRUD de pantalla de administración).
- Consultas paginadas con `Listar(..., paginacion)`.
- Verificaciones de unicidad en `ValidacionesDeNegocio()`.

**No usar** el repositorio para lógica de negocio de orquestación — eso va en `IServicioOrquestador`.

## Acciones de comando de dispositivo

Para acciones que ejecutan comandos en dispositivos (abrir barrera, tomar foto, etc.), delegar siempre a `servicio.Ejecutar(new EjecutarXxx { ... })`:

```csharp
[HttpPost]
public ActionResult EjecutarAccion(string codigoDispositivo, string parametro)
{
    var resultado = servicio.Ejecutar(new ComandoEjecutar
    {
        Comando = new EjecutarNuevaAccion
        {
            CodigoDispositivo = codigoDispositivo,
            ParametroObligatorio = parametro
        }
    });
    return Json(resultado);
}
```

## Vistas
- Ubicar en `Views/{TipoDispositivo}/`.
- Las vistas parciales (tablas de listado) se retornan con `return View("Listar", ...)` y se cargan vía AJAX con `[AjaxOnly]`.

## Prohibido en controllers
- Instanciar o usar drivers directamente.
- Acceder a `OrquestadorDbContext` directamente (usar `IRepositorio` vía `BaseController`).
- Lógica de negocio que debería estar en un `ProcesadorComando`.
- Validaciones que no sean validaciones de formato/unicidad de UI.
