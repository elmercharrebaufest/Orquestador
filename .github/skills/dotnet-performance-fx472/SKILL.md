---
name: dotnet-performance-fx472
description: >
  Análisis de patrones de performance para .NET Framework 4.7.2. Cubre strings, LINQ, EF6, async y allocations.
  Solo incluye patrones aplicables a este framework — excluye explícitamente APIs de .NET 6+/8+ que no están disponibles.
  Usar cuando se revisa código en busca de mejoras de rendimiento.
---

# Performance — .NET Framework 4.7.2

> **Alcance**: Solo patrones aplicables a **.NET Framework 4.7.2**. Las sugerencias de `Span<T>`, `Memory<T>`, `FrozenDictionary`, `SearchValues`, `[GeneratedRegex]` y `TimeProvider` **no aplican a este proyecto** — ver sección "No aplica" al final.

---

## Strings

### `StringBuilder` en loops — no `+` ni `+=`
```csharp
// ❌ O(n²) allocations
string resultado = "";
foreach (var item in items)
    resultado += item.ToString() + ", ";

// ✅
var sb = new StringBuilder();
foreach (var item in items)
    sb.Append(item).Append(", ");
string resultado = sb.ToString();
```
**Umbral**: a partir de 3+ concatenaciones en loop o método llamado frecuentemente.

### `string.Compare` con `StringComparison` explícito
```csharp
// ❌ Cultura-sensible e impredecible en servidores con locale distinto
if (codigo.ToLower() == otro.ToLower()) ...

// ✅ Explícito, sin allocations de nuevas strings
if (string.Equals(codigo, otro, StringComparison.OrdinalIgnoreCase)) ...
if (codigo.IndexOf("ABC", StringComparison.OrdinalIgnoreCase) >= 0) ...
```
Los códigos de dispositivo siempre se almacenan en **UPPERCASE** (garantizado por el setter de `Comando.CodigoDispositivo`). En comparaciones de código, usar `StringComparison.Ordinal` — no hay variaciones de cultura.

### `string.Format` vs interpolación
Ambos son equivalentes en .NET Framework; preferir interpolación `$"..."` por legibilidad, pero nunca interpolar dentro de loops pesados — usar `StringBuilder`.

---

## LINQ

### No ejecutar LINQ dentro de event handlers de drivers
Los event handlers se disparan por cada evento de dispositivo (puede ser cientos por segundo):
```csharp
// ❌ Allocación de iterador + posible N+1 por cada evento
void OnEventoDriver(object sender, EventoDriverEventArgs e)
{
    var suscripcion = suscripciones.Where(s => s.Dispositivo == e.Codigo).FirstOrDefault();
}

// ✅ Usar Dictionary para lookup O(1)
private readonly Dictionary<string, Suscripcion> suscripcionesPorDispositivo;

void OnEventoDriver(object sender, EventoDriverEventArgs e)
{
    suscripcionesPorDispositivo.TryGetValue(e.Codigo, out var suscripcion);
}
```

### Evitar `.ToList()` innecesario que materializa colecciones grandes
```csharp
// ❌ Materializa toda la lista solo para iterar
foreach (var d in repositorio.Listar<Dispositivo>().ToList())
    Procesar(d);

// ✅ IList<T> ya es materializado; si Listar() retorna IList no hace falta ToList()
foreach (var d in repositorio.Listar<Dispositivo>())
    Procesar(d);
```

### Múltiples enumeraciones de la misma query — materializar una vez
```csharp
// ❌ IQueryable ejecutado dos veces → dos roundtrips a la DB
var query = context.Set<Dispositivo>().Where(d => d.Activo);
int count = query.Count();      // query 1
var lista = query.ToList();     // query 2

// ✅ Materializar una vez
var lista = context.Set<Dispositivo>().Where(d => d.Activo).ToList();
int count = lista.Count;
```

---

## Entity Framework 6

### `.AsNoTracking()` en consultas de solo lectura
El change tracker de EF tiene un costo mensurable con entidades grandes o listados largos:
```csharp
// ✅ Para listados de configuración que solo se muestran
context.Set<ConfigDispositivo>().AsNoTracking().Where(...).ToList();
```
`RepositorioEF.ObtenerNoTracking<T>()` ya lo implementa — usarlo en lugar del `Obtener<T>()` genérico cuando no se va a modificar.

### Cargar solo las columnas necesarias con proyección
```csharp
// ❌ Carga la entidad completa + sus relaciones si hay lazy loading activo
var codigos = repositorio.Listar<Dispositivo>().Select(d => d.Codigo).ToList();
// Con EF esto funciona pero carga todas las props de Dispositivo primero si se usa Set<T>()

// ✅ Proyección directa — EF genera SELECT Codigo FROM Dispositivos
var codigos = context.Set<Dispositivo>().Select(d => d.Codigo).ToList();
```

### No hacer Include de relaciones que no se necesitan
```csharp
// ❌ Carga ConfigBarrera aunque solo se use Dispositivo.Nombre
context.Set<Dispositivo>().Include(d => d.ConfigBarrera).ToList();

// ✅ Solo incluir lo necesario
context.Set<Dispositivo>().Select(d => new { d.Id, d.Nombre }).ToList();
```

---

## Async / Await

### `.Result` y `.Wait()` causan deadlock en contexto WCF/ASP.NET
Este es el anti-patrón de mayor impacto en el proyecto:
```csharp
// ❌ Deadlock en WCF sync context o ASP.NET request thread
var datos = ObtenerAsync().Result;
tarea.Wait();

// ✅ Async all the way
var datos = await ObtenerAsync().ConfigureAwait(false);
```
**Regla**: si el método llama código async, debe ser `async` él también. No mezclar sync y async.

### No crear `Task.Run` innecesario para "hacer async" código sync
```csharp
// ❌ Solo mueve el bloqueo a un thread pool thread — no es async real
public async Task<string> ObtenerAsync()
{
    return await Task.Run(() => ObtenerSync());
}

// ✅ Si la operación es inherentemente sync (CPU-bound en servicio), déjala sync
public string Obtener() => ObtenerSync();
```

---

## Allocations y Boxing

### Boxing con colecciones no genéricas — evitar
```csharp
// ❌ ArrayList/Hashtable boxean value types (int, bool, enum)
ArrayList lista = new ArrayList();
lista.Add(42); // boxing

// ✅ Colecciones genéricas — cero boxing
List<int> lista = new List<int>();
lista.Add(42);
```
En este proyecto los value types más comunes en colecciones son `int` (IDs), `bool` (estados), y enums de tipo de dispositivo.

### Closures que capturan objetos grandes en event handlers
```csharp
// ❌ El closure retiene toda la referencia al servicio mientras el lambda vive
driver.EventoDriver += (s, e) => servicioOrquestador.Notificar(e.Datos);

// ✅ Si el handler puede des-registrarse, usar método nombrado para poder quitar la suscripción
driver.EventoDriver += OnEventoDriver;
// y al destruir:
driver.EventoDriver -= OnEventoDriver;
```

### Evitar `params` en métodos llamados frecuentemente
```csharp
// ❌ Cada llamada aloca un array aunque sea con un solo argumento
void LoguearEventos(params string[] eventos) { ... }

// ✅ Sobrecargar para el caso común
void LoguearEvento(string evento) { ... }
void LoguearEventos(IEnumerable<string> eventos) { ... }
```

---

## Regex

### Compilar Regex que se usan repetidamente
```csharp
// ❌ Nuevo objeto Regex por cada llamada
bool esValido = Regex.IsMatch(input, @"^\d{4}$");

// ✅ Compilado como campo estático readonly
private static readonly Regex PatronCodigo = new Regex(@"^\d{4}$", RegexOptions.Compiled);
bool esValido = PatronCodigo.IsMatch(input);
```

---

## Severidad

| Severidad | Descripción |
|-----------|-------------|
| 🔴 Crítico | Deadlock async (`.Result`/`.Wait()`), N+1 en loops de events |
| 🟠 Alto | `string +` en loops, LINQ en hot paths de drivers |
| 🟡 Moderado | ToList() innecesario, boxing, missing AsNoTracking |
| 🟢 Bajo | Regex sin compilar, params en métodos frecuentes |

---

## No aplica a este proyecto (.NET Framework 4.7.2)

Las siguientes APIs/patrones **requieren .NET 6 o superior** y **NO deben sugerirse** al revisar este código:

| API / Feature | Requiere |
|---|---|
| `Span<T>`, `Memory<T>` para buffers | .NET Core 2.1+ |
| `ArrayPool<T>` | .NET Core 2.1+ |
| `FrozenDictionary<K,V>`, `FrozenSet<T>` | .NET 8+ |
| `SearchValues<T>` | .NET 8+ |
| `[GeneratedRegex]` attribute | .NET 7+ |
| `TimeProvider` abstraction | .NET 8+ |
| `CollectionsMarshal` | .NET 5+ |
| `string.Create(length, state, action)` | .NET Core 2.1+ |
| Primary constructors en clases | C# 12 / .NET 8+ |
| Collection expressions `[1, 2, 3]` | C# 12 / .NET 8+ |
| `List<T>` → `TryGetNonEnumeratedCount` | .NET 6+ |
