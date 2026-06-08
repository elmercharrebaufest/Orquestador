---
applyTo: "**/Molinos.Orquest.Repositorio/**/*.cs"
---

# Reglas de la capa Repositorio

## Principio fundamental
Este proyecto es la única puerta de entrada a la base de datos. Expone un contrato genérico (`IRepositorio`) que abstrae completamente Entity Framework del resto de la aplicación. Solo puede depender de `Molinos.Orquest.Dominio` y Entity Framework.

## Contrato `IRepositorio`

- **No crear interfaces de repositorio por entidad** (ej.: no `IRepositorioDispositivo`, no `IRepositorioSuscripcion`). El contrato es uno solo y genérico.
- Las operaciones de consulta reciben `Expression<Func<TEntidad, bool>>` — el caller construye los predicados, el repositorio solo los ejecuta.
- Si se necesita una nueva operación de acceso a datos, agregarla como método en `IRepositorio` y su implementación en `RepositorioEF`.

## `RepositorioEF`

- Es la única implementación de `IRepositorio`. No crear implementaciones alternativas salvo para testing (usar mocks).
- Recibe `DbContext` por constructor (inyectado por Ninject).
- Scope: **Transient** — una instancia por unidad de trabajo. No compartir instancias entre requests.

## `OrquestadorDbContext`

- Es el único `DbContext` del proyecto.
- Agregar nuevas entidades como `DbSet<TEntidad>` en esta clase.
- El esquema de la base de datos **no se gestiona con migraciones EF** — se gestiona con el proyecto `Molinos.Orquest.Database`. Solo agregar el `DbSet<>` aquí; el DDL va en el proyecto de base de datos.

```csharp
// En OrquestadorDbContext, al agregar un nuevo tipo de dispositivo:
public DbSet<ConfigNuevoTipo> ConfiguracionesNuevoTipo { get; set; }
```

## `IRepositorioFactory`

- Existe para crear instancias de `IRepositorio` con scope controlado (especialmente en el Windows Service donde no hay request HTTP).
- Usar `repositorioFactory.Repositorio()` para obtener una instancia; disponer al terminar la unidad de trabajo.
- No usar `IRepositorio` directamente como singleton.

## Excepciones

- `EntidadReferenciadaException`: se lanza cuando se intenta eliminar una entidad que tiene referencias activas. Reutilizar en lugar de crear excepciones nuevas para el mismo escenario.

## Prohibido en esta capa
- Lógica de negocio o validaciones de dominio.
- Referencias a `Molinos.Orquest.Servicios`, `Molinos.Orquest.Drivers`, `Molinos.Orquest.Web`.
- Queries SQL en strings (usar LINQ sobre `DbSet`).
- Múltiples `DbContext` en el mismo proyecto.
- Repositorios específicos por entidad que dupliquen la funcionalidad genérica de `IRepositorio`.
