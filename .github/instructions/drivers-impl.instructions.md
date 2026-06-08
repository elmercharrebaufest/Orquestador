---
applyTo: "**/Molinos.Orquest.DriversImpl/**/*.cs"
---

# Reglas de la capa DriversImpl (implementaciones concretas)

## Principio fundamental
Este proyecto contiene **todas las implementaciones físicas de los drivers**. Cada clase es la traducción entre el contrato del dominio y un protocolo de hardware concreto (TCP, UDP, serial, HTTP, etc.). Puede depender de `Molinos.Orquest.Dominio`, `Molinos.Orquest.Drivers`, y paquetes NuGet de comunicación. **No puede depender de Servicios, Repositorio, Web ni Dependencias**.

## Estructura de una implementación de driver

### Clase base obligatoria
Toda implementación hereda de `DriverBase` e implementa la interface específica:

```csharp
public class DriverNuevoTipo : DriverBase, IDriverNuevoTipo
{
    // Retorna el tipo de ConfigDispositivo que este driver espera
    public override Type TipoDispositivo => typeof(ConfigNuevoTipo);

    public override void Inicializar(string codigo, ConfigDispositivo configuracion)
    {
        var config = (ConfigNuevoTipo)configuracion;
        // inicializar conexión con datos de config
    }

    public override void VerificarDispositivo()
    {
        // ping / health check al dispositivo físico
        // lanzar ConexionDispositivoDriverException si no responde
    }
}
```

### Convenciones de naming
- Nombre: `Driver{TipoDispositivo}` para implementación única, `Driver{TipoDispositivo}{Variante}` para múltiples (ej.: `DriverBarreraPlc`, `DriverBarreraItc`, `DriverCabezalIPContinuo`, `DriverCabezalIPPorDemanda`).
- Clases de soporte internas (helpers de protocolo) se colocan en `Helpers/`.
- Clases dummy para testing van con sufijo `Dummy` (ej.: `DriverCamaraALPRDummy`).

## Implementación de `Inicializar()`
- Castear `configuracion` al tipo concreto `Config{TipoDispositivo}` al inicio del método.
- Si el cast falla, el runtime lanzará `InvalidCastException`; esto es un error de configuración, no hay que enmascararlo.
- Guardar los parámetros de conexión (IP, puerto, timeouts) como campos privados.
- No abrir la conexión aquí; abrirla lazy en la primera operación o en `VerificarDispositivo()`.

## Manejo de errores
Usar siempre excepciones de `Molinos.Orquest.Drivers`:

| Situación | Excepción a lanzar |
|-----------|-------------------|
| Dispositivo no responde / timeout de conexión | `ConexionDispositivoDriverException` |
| Respuesta del dispositivo en formato inesperado | `FormatoRespuestaDriverException` |
| Parámetros del comando incompletos o inválidos | `ComandoDriverException` |
| Error de lectura/escritura en el canal | `LecturaEscrituraDriverException` |
| Error con mensaje legible para el usuario | `DriverConMensajeException` |

**Nunca** lanzar `Exception` directa ni `ApplicationException`.

## Eventos de dispositivo
Para notificar eventos asíncronos (ej.: lectura de tag, detección de vehículo):

```csharp
protected void NotificarEvento(string codigoEvento, object datos)
{
    OnEventoDriver(new EventoDriverEventArgs(codigoEvento, datos));
}
```

Usar las constantes de `CodigosEventos` del proyecto `Drivers`.

## Comunicación de red
- `TcpCommandClient` y `TcpCommandListener` ya existen para TCP sincrónico — reutilizarlos.
- `UdpCommandClient` para UDP.
- Los helpers de bytes están en `ExtensionesByte` y `ExtensionesTcpClient`.

## `MantenerConectado()`
- Retornar `true` si el driver debe mantener una conexión persistente (el orquestador llamará a `VerificarDispositivo()` periódicamente).
- Retornar `false` (default en `DriverBase`) si la conexión es por demanda.

## Prohibido en esta capa
- Inyectar o usar `IRepositorio`, `DbContext` o `IServicioOrquestador`.
- Lógica de negocio de orquestación (suscripciones, workflows).
- Acceso a `System.Web` o contexto HTTP.
- Llamar a otros drivers directamente (el `DriverGrupoBarrera` coordina vía interfaces, no instancias concretas).
