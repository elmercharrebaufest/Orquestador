---
applyTo: "**/Molinos.Orquest.Dominio/**/*.cs"
---

# Reglas de la capa Dominio

## Principio fundamental
Este proyecto es el núcleo de la arquitectura. **No puede referenciar ningún otro proyecto del solution** (ni Servicios, ni Repositorio, ni Drivers, ni DriversImpl, ni Web). Solo puede depender de paquetes NuGet externos.

## Entidades (`Entidades/`)

- Cada entidad es una clase POCO con propiedades `virtual` (requerido por EF lazy loading).
- Usar Data Annotations para validación y mapeo EF (`[Key]`, `[Required]`, `[Display]`, `[Table]`, etc.).
- Los mensajes de validación deben usar `ResourceType = typeof(Textos)` — nunca strings literales.
- Los códigos de dispositivo se almacenan y comparan en **UPPERCASE**. El setter de `CodigoDispositivo` en `Comando` ya lo garantiza; replicar ese patrón si se crean propiedades de código en entidades.
- Las entidades no contienen lógica de negocio. Si una propiedad calculada es necesaria, marcarla con `[NotMapped]`.

## Configuraciones de dispositivo (`Entidades/Config*.cs`)

- Toda configuración de dispositivo hereda de `ConfigDispositivo`.
- Nombre: `Config{TipoDispositivo}` (ej.: `ConfigBarrera`, `ConfigCamara`).
- Anotar con `[Table("Config{TipoDispositivo}")]`.
- La clave primaria es compartida con `Dispositivo` via `[Key, ForeignKey("Dispositivo")]`.
- Propiedades opcionales de FK que no van a la DB se marcan `[NotMapped]`.

```csharp
[Table("ConfigNuevoTipo")]
public class ConfigNuevoTipo : ConfigDispositivo
{
    [Required(...)]
    public virtual string PropiedadRequerida { get; set; }

    [NotMapped]
    public virtual int? IdRelacionOpcional { get; set; }
}
```

## Comandos (`Comandos/`)

- Toda operación sobre un dispositivo se modela como un comando que hereda de `Comando`.
- Nombres: `Ejecutar{Acción}` para acciones de dispositivo (ej.: `EjecutarAperturaBarrera`), `Comando{Acción}` para operaciones de orquestación (ej.: `ComandoSuscribir`).
- Decorar con `[DataContract]` (WCF serialization).
- Cada propiedad que se transmite lleva `[DataMember]`.
- Los comandos no contienen lógica. Son objetos de datos puros.

```csharp
[DataContract]
public class EjecutarNuevaAccion : Comando
{
    [DataMember]
    public string ParametroRequerido { get; set; }
}
```

## Resultados (`Resultados/`)

- Cada comando tiene su resultado correspondiente heredando de `ResultadoComando`.
- Nombre: `Resultado{Acción}` (ej.: `ResultadoTomarFoto`).
- Decorar con `[DataContract]`, propiedades con `[DataMember]`.
- Los resultados no contienen lógica. `Mensaje` (con `Codigos.*`) ya está en la base.

## DTOs (`Dtos/`)

- Solo para transferencia de datos entre capas o hacia clientes externos.
- Sin lógica, sin referencias a EF, sin Data Annotations de validación MVC.
- Nombre: `{Concepto}Dto` (ej.: `DispositivoDto`, `CamaraDto`).

## Excepciones

- Ubicación: raíz del proyecto `Molinos.Orquest.Dominio`.
- Nombre: `{Concepto}Exception` (ej.: `DispositivoNoEncontradoException`).
- Heredar de `Exception` o de otra excepción de dominio existente.

## Consultas (`Consultas/`)

- Clases de soporte para paginación y ordenamiento: `Paginacion`, `ListaPaginada<T>`, `DirOrden`, `Expresiones`.
- No agregar lógica de acceso a datos aquí.

## Prohibido en esta capa
- Referencias a `System.Data.Entity` (EF), Ninject, WCF server-side, `System.Web`, o cualquier proyecto del solution.
- Constructores con lógica de negocio.
- Llamadas a servicios, repositorios o drivers.
