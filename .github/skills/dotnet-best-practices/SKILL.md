---
name: dotnet-best-practices
description: >
  Buenas prácticas de codificación específicas para este proyecto: .NET Framework 4.7.2, Entity Framework 6,
  WCF, Ninject, ASP.NET MVC 5, NUnit + Moq. Usar cuando se revisa o escribe código nuevo en cualquier capa
  del Orquestador para verificar que cumple con los estándares del proyecto.
---

# Buenas Prácticas — Orquestador (.NET Framework 4.7.2)

Stack del proyecto: **.NET Framework 4.7.2 · Entity Framework 6 · WCF · Ninject · ASP.NET MVC 5 · NUnit 3 · Moq**

---

## Entity Framework 6

### Consultas de solo lectura → usar `AsNoTracking()`
Cuando los datos solo se muestran (sin modificar), evitar el overhead del change tracker:
```csharp
// ✅ Correcto — consulta de solo lectura
context.Set<TEntidad>().AsNoTracking().SingleOrDefault(condicion);

// ❌ Incorrecto — EF rastrea innecesariamente si no se va a guardar
context.Set<TEntidad>().SingleOrDefault(condicion);
```
El `RepositorioEF` ya expone `ObtenerNoTracking<T>()`. Usarlo en controllers y servicios que solo leen.

### Eager loading obligatorio — nunca lazy loading implícito en servicios
Cuando se necesitan relaciones, cargarlas explícitamente con `.Include()`:
```csharp
// ✅ Correcto
context.Set<Dispositivo>().Include(d => d.ConfigBarrera).SingleOrDefault(...);

// ❌ Incorrecto — acceder a propiedad de navegación fuera del contexto lanza excepción
var d = repositorio.Obtener<Dispositivo>(...);
var nombre = d.ConfigBarrera.Nombre; // LazyLoadingException si el contexto ya se eliminó
```

### Proyecciones > entidades completas
Si solo se necesitan algunos campos, usar proyección en lugar de cargar toda la entidad:
```csharp
// ✅
repositorio.Obtener<Dispositivo, string>(d => d.Id == id, d => d.Codigo);

// ❌ — carga todos los campos solo para usar uno
var dispositivo = repositorio.Obtener<Dispositivo>(d => d.Id == id);
return dispositivo.Codigo;
```

### N+1 — detectar y eliminar
```csharp
// ❌ N+1: consulta dentro de loop
foreach (var barrera in repositorio.Listar<ConfigBarrera>())
{
    var disp = repositorio.Obtener<Dispositivo>(d => d.Id == barrera.Id); // query por cada iteración
}

// ✅ Una sola consulta con Include
var barreras = context.Set<ConfigBarrera>().Include(b => b.Dispositivo).ToList();
```

---

## WCF — Contratos y Serialización

### `[DataContract]` / `[DataMember]` obligatorios en DTOs de servicio
```csharp
[DataContract]
public class ResultadoLectura
{
    [DataMember]
    public string Valor { get; set; }

    [DataMember]
    public DateTime Timestamp { get; set; }
}
```

### `[KnownType]` para jerarquías de comandos
Los tipos derivados de `Comando` deben registrarse con `[KnownType]` para que WCF los serialice correctamente. El método `TiposDeComandos()` ya existe — agregar el nuevo tipo ahí, no crear otra anotación suelta.

### No exponer excepciones internas en contratos WCF
WCF no debe propagar `Exception` raw al cliente. Usar `FaultException<T>` o capturar y transformar:
```csharp
// ❌ Incorrecto — rompe el canal WCF
throw new InvalidOperationException("Error interno");

// ✅ Correcto
throw new FaultException("Descripción del error para el cliente");
```

### Vincular los servicios a interfaces, no a implementaciones
El cliente siempre habla con la interfaz (`IServicioOrquestador`). Nunca inyectar `ServicioOrquestador` directamente en clientes.

---

## Ninject — Inyección de Dependencias

### Scopes correctos

| Tipo | Scope | Razón |
|------|-------|-------|
| Servicios de orquestación (`IServicioOrquestador`, `IAdministradorSuscripciones`) | `InSingletonScope()` | Estado compartido, costosos de crear |
| Factories (`IDriverFactory`, `IRepositorioFactory`) | `InSingletonScope()` | Sin estado mutable |
| `DbContext` (`OrquestadorDbContext`) | `InTransientScope()` | Una instancia por operación — nunca singleton |
| `IRepositorio` (`RepositorioEF`) | `InTransientScope()` | Depende del DbContext |
| Drivers | `InSingletonScope()` | Representan hardware físico |

### No usar Service Locator
```csharp
// ❌ Incorrecto — antipatrón Service Locator
var repositorio = kernel.Get<IRepositorio>();

// ✅ Correcto — inyección por constructor
public class MiServicio
{
    private readonly IRepositorio repositorio;
    public MiServicio(IRepositorio repositorio) { this.repositorio = repositorio; }
}
```

### Registrar en el módulo correcto
- Bindings del **Windows Service** → `OrquestNinjectModule`
- Bindings específicos de la **Web** → `OrquestWebNinjectModule`
- No duplicar bindings entre módulos

---

## Async / Await (.NET Framework 4.7.2)

### Nunca `.Result` ni `.Wait()` en código que corre en contexto sincronizador
```csharp
// ❌ Deadlock garantizado en WCF/ASP.NET sync context
var resultado = ObtenerDatosAsync().Result;
var datos = task.Wait();

// ✅ Si el método puede ser async, hacerlo async
var resultado = await ObtenerDatosAsync();
```

### `ConfigureAwait(false)` en librerías/servicios
```csharp
// ✅ En métodos de Servicios, Repositorio, Drivers — evitar capturar el contexto
var datos = await repositorio.ObtenerAsync(...).ConfigureAwait(false);
```

### `Task.WhenAll` para operaciones paralelas independientes
```csharp
// ✅ Lanzar ambas tareas y esperar juntas
var tareaA = repositorio.Listar<Dispositivo>().ConfigureAwait(false);
var tareaB = repositorio.Listar<Suscripcion>().ConfigureAwait(false);
await Task.WhenAll(tareaA, tareaB);
```

---

## Manejo de Excepciones

### Usar la jerarquía existente del proyecto
No lanzar `Exception` o `ApplicationException` directamente. El proyecto tiene:

```
Exception
└── DriverException
    ├── ComandoDriverException        — comando inválido para el driver
    └── ConexionDispositivoDriverException — fallo de conexión con el dispositivo
DispositivoNoEncontradoException      — código de dispositivo no registrado
DriverNoEncontradoException           — no hay driver para el tipo de dispositivo
SuscripcionNoEncontradaException      — suscripción no existe
TipoDispositivoIncorrectoException    — comando enviado al tipo de dispositivo equivocado
TipoDriverIncorrectoException         — driver no implementa la interfaz requerida
```

### No swallow exceptions
```csharp
// ❌ Tiempo-bomba — el error desaparece silenciosamente
try { driver.VerificarDispositivo(); }
catch (Exception) { }

// ✅ Al menos loguear; relanzar si el caller necesita saberlo
try { driver.VerificarDispositivo(); }
catch (ConexionDispositivoDriverException ex)
{
    log.Warn($"Dispositivo {codigo} no responde: {ex.Message}");
    throw;
}
```

### No usar excepciones para control de flujo
```csharp
// ❌
try { return repositorio.Obtener<Dispositivo>(d => d.Codigo == codigo); }
catch (InvalidOperationException) { return null; } // SingleOrDefault ya devuelve null

// ✅
return repositorio.Obtener<Dispositivo>(d => d.Codigo == codigo); // SingleOrDefault retorna null si no existe
```

---

## Event Handlers en Drivers (.NET Framework 4 pattern)

Siempre capturar el handler antes de la comprobación de null para evitar race condition:
```csharp
// ✅ Patrón correcto (thread-safe en .NET 4)
protected virtual void OnEventoDriver(EventoDriverEventArgs e)
{
    EventHandler<EventoDriverEventArgs> handler = EventoDriver;
    if (handler != null)
    {
        handler(this, e);
    }
}

// ❌ Race condition: EventoDriver puede volverse null entre la comprobación y la invocación
if (EventoDriver != null) EventoDriver(this, e);
```

---

## Tests con NUnit 3 + Moq

### Estructura de clase de test
```csharp
[TestFixture]
public class MiServicioTest
{
    private MiServicio target;
    private Mock<IDependencia> dependenciaMock;

    [SetUp]
    public void SetUp()
    {
        dependenciaMock = new Mock<IDependencia>();
        target = new MiServicio(dependenciaMock.Object);
    }

    [Test]
    public void MetodoQueSeTestea_CuandoEscenario_DeberiaResultado()
    {
        // Arrange
        dependenciaMock.Setup(d => d.Obtener(It.IsAny<int>())).Returns(new Entidad());

        // Act
        var resultado = target.MetodoQueSeTestea(1);

        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoEsperado));
    }
}
```

### Naming: `Metodo_Escenario_Resultado`
```csharp
// ✅
public void VerificarDispositivo_CuandoNoHayConexion_LanzaConexionDispositivoDriverException()
public void EjecutarComando_CuandoComandoEsNull_LanzaArgumentNullException()
public void Listar_CuandoNoHayRegistros_RetornaListaVacia()

// ❌
public void TestVerificar()
public void Test1()
```

### Mocks: setup antes del Act, verify después del Assert
```csharp
// ✅ Verificar que se llamó al colaborador
dependenciaMock.Verify(d => d.Guardar(It.IsAny<Entidad>()), Times.Once);

// ❌ No hacer Verify innecesario de cosas no relevantes al test
dependenciaMock.Verify(d => d.Obtener(...), Times.Once); // solo si es parte del comportamiento testeable
```

### Siempre al menos una aserción significativa
```csharp
// ❌ No testea nada — el test nunca falla
[Test]
public void EjecutarComando_NoLanzaExcepcion()
{
    target.EjecutarComando(new ComandoEjecutar());
    // sin asserts
}

// ✅
Assert.DoesNotThrow(() => target.EjecutarComando(new ComandoEjecutar()));
// o verificar el estado resultante
```
