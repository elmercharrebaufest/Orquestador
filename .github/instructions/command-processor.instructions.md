---
applyTo: "**/Procesamiento/**/*.cs"
---

# Reglas del patrón Command + Processor

## Principio fundamental
Toda operación sobre un dispositivo sigue el patrón **Comando → Procesador**. El comando describe *qué* hacer, el procesador sabe *cómo* hacerlo delegando al driver. Este patrón garantiza que la lógica de orquestación nunca esté ni en el controlador ni en el driver.

## Relación entre archivos por cada operación nueva

| Artefacto | Proyecto | Carpeta | Naming |
|-----------|----------|---------|--------|
| Comando | `Dominio` | `Comandos/` | `Ejecutar{Acción}` |
| Resultado | `Dominio` | `Resultados/` | `Resultado{Acción}` |
| Procesador | `Servicios` | `Procesamiento/` | `ProcesadorEjecutar{Acción}` |

Los tres artefactos deben crearse juntos. Nunca crear uno sin los otros dos.

## Comando (`Dominio/Comandos/Ejecutar{Acción}.cs`)

```csharp
[DataContract]
public class EjecutarNuevaAccion : Comando
{
    [DataMember]
    public string ParametroObligatorio { get; set; }

    [DataMember]
    public int? ParametroOpcional { get; set; }
}
```

- Hereda siempre de `Comando`.
- `[DataContract]` en la clase, `[DataMember]` en cada propiedad que se serializa.
- Sin lógica, sin métodos que no sean propiedades.

## Resultado (`Dominio/Resultados/Resultado{Acción}.cs`)

```csharp
[DataContract]
public class ResultadoNuevaAccion : ResultadoComando
{
    [DataMember]
    public string DatoDeLaRespuesta { get; set; }
}
```

- Hereda de `ResultadoComando`.
- `Mensaje` (con el código de resultado) ya está en la clase base; no redefinirlo.
- Si la operación no devuelve datos adicionales, heredar de `ResultadoComando` sin agregar propiedades.

## Procesador (`Servicios/Procesamiento/ProcesadorEjecutar{Acción}.cs`)

```csharp
public class ProcesadorEjecutarNuevaAccion
    : ProcesadorComando<EjecutarNuevaAccion, ResultadoNuevaAccion>
{
    public ProcesadorEjecutarNuevaAccion(ILogger log) : base(log) { }

    protected override ResultadoNuevaAccion Ejecutar(
        EjecutarNuevaAccion comando,
        Dispositivo dispositivo,
        IDriver driver)
    {
        var driverEspecifico = (IDriverNuevoTipo)driver;

        // Lógica de negocio aquí
        var resultado = driverEspecifico.EjecutarAccion(comando.ParametroObligatorio);

        return new ResultadoNuevaAccion
        {
            Mensaje = new Mensaje(Codigos.OK, Textos.OK, dispositivo.Codigo),
            DatoDeLaRespuesta = resultado
        };
    }
}
```

### Reglas del procesador
- El único método a implementar es `Ejecutar()` — la clase base `ProcesadorComando<,>` maneja todo el manejo de excepciones de driver.
- Castear `IDriver` al tipo específico `IDriver{TipoDispositivo}` al inicio. Si el cast es incorrecto, es un error de configuración y debe fallar rápido.
- Usar `Codigos.*` para los códigos de resultado (nunca strings literales).
- Usar `Textos.*` para los mensajes (recursos localizados).
- El procesador no accede a `IRepositorio` directamente. Si necesita datos, deben venir en el comando o en la entidad `Dispositivo` ya hidratada.
- Inyectar solo `ILogger` en el constructor (lo que la clase base requiere).

## Registro en ProcesadorFactory
Después de crear el procesador, registrarlo en `Servicios/Impl/ProcesadorFactory.cs`. El factory resuelve el procesador correcto según el tipo de comando. Seguir el patrón ya establecido de la clase.

## Procesadores de comandos de orquestación (`Comando{Acción}`)
Para operaciones de ciclo de vida del orquestador (suscribir, cancelar, depurar) el naming es `ProcesadorComando{Acción}` (ej.: `ProcesadorComandoSuscribir`). El patrón de implementación es el mismo.

## Prohibido en procesadores
- Acceso directo a `IRepositorio` o `DbContext`.
- Instanciar drivers manualmente (`new Driver...`).
- Lógica de routing o selección de driver.
- Try/catch de excepciones de driver — la clase base `ProcesadorComando<,>` ya las maneja y convierte a `ResultadoComando` con el mensaje adecuado.
