---
applyTo: "**/Molinos.Orquest.Dependencias/**/*.cs"
---

# Reglas de la capa Dependencias (DI)

## Principio fundamental
Este proyecto es el **único lugar donde se conectan las interfaces con sus implementaciones**. Contiene los módulos Ninject y nada más. No contiene lógica de negocio, ni factories propias, ni helpers de utilidad.

## Dos módulos, dos hosts

| Módulo | Host | Qué registra |
|--------|------|--------------|
| `OrquestNinjectModule` | Windows Service (`Servidor`) | Todo: EF, repositorio, servicios, factories, scheduler |
| `OrquestWebNinjectModule` | Web App (`Web`) | Solo lo necesario para la web: EF, repositorio, factory proxy WCF |

**Nunca mezclar registros de ambos módulos.** Si un binding es necesario en ambos hosts, registrarlo en ambos módulos por separado.

## Scopes obligatorios

| Binding | Scope | Módulo |
|---------|-------|--------|
| `DbContext` → `OrquestadorDbContext` | `InTransientScope()` | Ambos |
| `IRepositorio` → `RepositorioEF` | `InTransientScope()` | Ambos |
| `IRepositorioFactory` → `RepositorioFactory` | `InSingletonScope()` | Ambos |
| `IDriverFactory` → `DriverFactory` | `InSingletonScope()` | Ambos |
| `IServicioOrquestador` → `ServicioOrquestador` | `InSingletonScope()` | Solo `OrquestNinjectModule` |
| `IAdministradorSuscripciones` → `AdministradorSuscripciones` | `InSingletonScope()` | Solo `OrquestNinjectModule` |
| `IProgramadorTareas` → `ProgramadorTareas` | `InSingletonScope()` | Solo `OrquestNinjectModule` |
| `IServicioOrquestador` (proxy WCF) | Channel factory | Solo `OrquestWebNinjectModule` |

**El repositorio y DbContext son siempre Transient.** Los servicios de orquestación son siempre Singleton.

## Patrón Decorator para `IAdministradorSuscripciones`

Cuando `maxNotificacions > 0`, `ControlDeNotificaciones` decora `AdministradorSuscripciones`. El patrón de binding ya está establecido:

```csharp
// Con decorator:
Bind<IAdministradorSuscripciones>()
    .To<AdministradorSuscripciones>()
    .WhenInjectedInto<ControlDeNotificaciones>()
    .InSingletonScope();
Bind<IAdministradorSuscripciones>()
    .To<ControlDeNotificaciones>()
    .InSingletonScope()
    .WithConstructorArgument("maxNotificacions", maxNotificacions);

// Sin decorator:
Bind<IAdministradorSuscripciones>().To<AdministradorSuscripciones>().InSingletonScope();
```

Si se agrega otro decorator, seguir este mismo patrón de `WhenInjectedInto`.

## Registrar un nuevo servicio o factory

1. Agregar el binding en **ambos módulos** si se usa en ambos hosts, o solo en el módulo del host correspondiente.
2. Determinar el scope correcto según la tabla de arriba.
3. Si la implementación requiere parámetros de configuración del `App.config`, leerlos desde `AppConfig` (diccionario disponible en `OrquestNinjectModule`) y pasarlos con `WithConstructorArgument`.

## `ExtensionesNinject.cs`

Contiene el método de extensión `BindChannelFactory<T>()` usado en `OrquestWebNinjectModule` para crear el proxy WCF. Si se necesita exponer un nuevo servicio WCF desde la Web, usar este mismo método de extensión.

## Prohibido en esta capa
- Lógica de negocio de ningún tipo.
- Código que no sea registros de bindings Ninject.
- Instanciar clases concretas fuera del contexto de un binding (`new ServicioOrquestador()` fuera de Ninject).
- Referenciar `Molinos.Orquest.Web` o `Molinos.Orquest.Servidor` (la dependencia es en sentido contrario).
