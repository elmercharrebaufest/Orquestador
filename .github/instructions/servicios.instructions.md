---
applyTo: "**/Molinos.Orquest.Servicios/**/*.cs"
---

# Reglas de la capa Servicios

## Principio fundamental
Este proyecto contiene los **contratos de servicio** (interfaces) y sus **implementaciones** (en `Impl/`). Es la capa de orquestación de negocio: coordina drivers, repositorio y lógica de suscripciones. Puede depender de `Dominio`, `Drivers` y `Repositorio`. **No puede depender de DriversImpl, Web ni Dependencias**.

## Estructura: interface en raíz, implementación en `Impl/`

```
Servicios/
  IServicioOrquestador.cs        ← contrato público
  IAdministradorSuscripciones.cs ← contrato público
  IDriverFactory.cs              ← contrato de factory
  Impl/
    ServicioOrquestador.cs       ← implementación
    AdministradorSuscripciones.cs
    DriverFactory.cs
  Procesamiento/
    IProcesadorComando.cs
    ProcesadorComando.cs (base abstracta)
    ProcesadorEjecutar*.cs
```

**La interface siempre va en la raíz del proyecto. La implementación siempre va en `Impl/`.** Nunca crear una implementación en la raíz ni una interface en `Impl/`.

## Servicios expuestos por WCF

Si el servicio se expone como endpoint WCF, la interface lleva:

```csharp
[ServiceContract(Namespace = "http://orquestador.molinos.com.ar")]
public interface IServicioNuevo
{
    [OperationContract]
    ResultadoXxx OperacionX(ComandoXxx comando);
}
```

- `Namespace` siempre `"http://orquestador.molinos.com.ar"`.
- Solo la interface lleva los atributos WCF, no la implementación.
- Los tipos de parámetros y retorno deben ser serializables (`[DataContract]`/`[DataMember]` en el Dominio).

## Factories

Existen cuatro factories establecidas. Si se necesita una nueva, seguir el mismo patrón:

| Interface | Implementación | Propósito |
|-----------|---------------|-----------|
| `IDriverFactory` | `DriverFactory` | Crear/resolver instancias de `IDriver` |
| `IProcesadorFactory` | `ProcesadorFactory` | Resolver `IProcesadorComando` por tipo de comando |
| `IRepositorioFactory` | `RepositorioFactory` | Crear instancias de `IRepositorio` con scope |
| `IServicioRemotoFactory` | `ServicioRemotoFactory` | Crear proxies WCF a orquestadores remotos |

- Todas las factories son **Singleton** (una instancia por aplicación).
- El factory no ejecuta lógica de negocio; solo crea o resuelve instancias.

## Scopes de vida

| Componente | Scope | Razón |
|-----------|-------|-------|
| `IServicioOrquestador` | Singleton | Mantiene estado de dispositivos activos |
| `IAdministradorSuscripciones` | Singleton | Mantiene el registro de suscripciones |
| `IDriverFactory`, factories | Singleton | Sin estado, costoso de crear |
| `IProgramadorTareas` | Singleton | Scheduler único por proceso |

## `IAdministradorSuscripciones` y el patrón Decorator

`ControlDeNotificaciones` es un decorator sobre `AdministradorSuscripciones`. Si se necesita agregar comportamiento transversal (throttling, logging, circuit breaker) a `IAdministradorSuscripciones`, hacerlo como decorator, no modificando la implementación base.

## Procesadores (ver también `command-processor.instructions.md`)

- `IProcesadorComando` y `IProcesadorComando<TCmd, TRes>` son los contratos en `Procesamiento/`.
- `ProcesadorComando<TCmd, TRes>` es la clase base abstracta que implementa el manejo de errores de driver.
- Solo sobrescribir `Ejecutar()` en los procesadores concretos.

## Prohibido en esta capa
- Instanciar drivers directamente (`new DriverBarreraPlc()`); siempre usar `IDriverFactory`.
- Acceder a `HttpContext` o cualquier concepto web.
- Referenciar `Molinos.Orquest.DriversImpl` (solo se conocen las interfaces de `Drivers`).
- Lógica de presentación o formateo para UI.
