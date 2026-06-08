---
applyTo: "**/Molinos.Orquest.Drivers/**/*.cs"
---

# Reglas de la capa Drivers (contratos)

## Principio fundamental
Este proyecto define **únicamente contratos (interfaces y excepciones)** para los drivers de dispositivos. No contiene ninguna implementación concreta. Solo puede depender de `Molinos.Orquest.Dominio`.

## Interfaces de driver

### Interface base
`IDriver` es la interface raíz. Toda interface de dispositivo específico hereda de ella:

```csharp
public interface IDriverNuevoTipo : IDriver
{
    // Métodos específicos del tipo de dispositivo
    ResultadoNuevoTipo EjecutarAccion(ParametroAccion parametro);
}
```

### Convenciones de naming
- Nombre: `IDriver{TipoDispositivo}` (ej.: `IDriverBarrera`, `IDriverCamara`, `IDriverSensor`).
- Un archivo por interface.
- El tipo de dispositivo en el nombre debe coincidir con el `Config{TipoDispositivo}` del Dominio.

### Qué incluir en la interface
- Solo las operaciones que el tipo de dispositivo soporta.
- Métodos que devuelven tipos del Dominio (`ResultadoXxx`, DTOs) o tipos primitivos.
- Eventos específicos del dispositivo si aplica (heredando el patrón de `EventoDriver`).

### Qué NO incluir
- Lógica de implementación.
- Referencias a protocolos concretos (TCP, UDP, serial).
- Dependencias de infraestructura.

## Excepciones de driver

La jerarquía existente es:
```
DriverException
├── ComandoDriverException         (comando mal formado / parámetros inválidos)
├── ConexionDispositivoDriverException  (fallo de conectividad)
├── FormatoRespuestaDriverException     (respuesta inesperada del dispositivo)
├── LecturaEscrituraDriverException     (error de I/O)
└── DriverConMensajeException           (error con mensaje para el usuario)
```

- Nuevas excepciones de driver van en este proyecto, heredando de `DriverException` o de la excepción más específica de la jerarquía.
- Nombre: `{Concepto}DriverException`.
- No crear excepciones genéricas; modelar el fallo concreto.

## Constantes y códigos (`CodigosEventos.cs`)

- Los códigos de eventos de driver se agregan en `CodigosEventos`.
- Usar constantes `string`, formato descriptivo en mayúsculas o PascalCase consistente con las existentes.

## Prohibido en esta capa
- Clases concretas (salvo `EventoDriverEventArgs` que es un DTO de evento).
- Referencias a `System.Data.Entity`, Ninject, `System.Web`, o `System.Net.Sockets`.
- Cualquier referencia a otros proyectos del solution excepto `Molinos.Orquest.Dominio`.
