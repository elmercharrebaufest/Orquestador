# Referencia: Recursos i18n

## Archivos involucrados

| Archivo | Propósito |
|---------|-----------|
| `Molinos.Orquest.Dominio\Recursos\Textos.resx` | Strings en español (idioma por defecto) |
| `Molinos.Orquest.Dominio\Recursos\Textos.en.resx` | Strings en inglés |
| `Molinos.Orquest.Dominio\Recursos\Textos.Designer.cs` | Clase generada — **se mantiene manualmente** (no hay T4 auto-gen) |

---

## Claves mínimas para un ABM

```xml
<!-- Textos.resx -->
<data name="{NombreDominio}_Titulo" xml:space="preserve">
  <value>Título legible de la entidad</value>
</data>
<data name="{NombreDominio}_Administrar" xml:space="preserve">
  <value>Texto corto para el menú (ej: Identificación Vehicular)</value>
</data>
<data name="{NombreDominio}_Administracion" xml:space="preserve">
  <value>Texto largo para títulos de página (ej: Configuración Identificación Vehicular)</value>
</data>
<data name="{NombreDominio}_Crear" xml:space="preserve">
  <value>Crear {Titulo}</value>
</data>
<data name="{NombreDominio}_Modificar" xml:space="preserve">
  <value>Modificar {Titulo}</value>
</data>
<data name="{NombreDominio}_InformacionGeneral" xml:space="preserve">
  <value>Información General</value>
</data>
<!-- Una entrada por campo del formulario -->
<data name="{NombreDominio}_Nombre" xml:space="preserve">
  <value>Nombre</value>
</data>
<data name="{NombreDominio}_Codigo" xml:space="preserve">
  <value>Código</value>
</data>
<data name="{NombreDominio}_DebeSeleccionarDispositivo" xml:space="preserve">
  <value>Debe seleccionar al menos un dispositivo</value>
</data>
```

Repetir las mismas claves en `Textos.en.resx` con los valores en inglés.

---

## Actualizar Textos.Designer.cs

Por cada clave nueva, agregar una propiedad estática siguiendo exactamente el patrón existente:

```csharp
/// <summary>
///   Looks up a localized string similar to "Título legible de la entidad".
/// </summary>
public static string {NombreDominio}_Titulo
{
    get
    {
        return ResourceManager.GetString("{NombreDominio}_Titulo", resourceCulture);
    }
}
```

### Importante

- El nombre de la propiedad debe coincidir **exactamente** con la clave del `.resx` (incluye guiones bajos)
- No usar `nameof` ni reflexión; el string literal debe ser idéntico a la clave
- Mantener el orden alfabético o agrupar por dominio para facilitar búsquedas

---

## Verificar antes de compilar

```powershell
Select-String "{NombreDominio}_" `
  "c:\Repositorios\MOA\Orquestador\Molinos.Orquest.Dominio\Recursos\Textos.Designer.cs"
```

Todos los nombres de clave en el `.resx` deben tener su propiedad en el `.Designer.cs`.
