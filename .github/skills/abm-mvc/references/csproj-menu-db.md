# Referencia: csproj, Menú y Database

## Registro en Molinos.Orquest.Web.csproj

Buscar el archivo en `Molinos.Orquest.Web\Molinos.Orquest.Web.csproj`.
Cada artefacto nuevo debe estar registrado. Verificar con `grep_search` antes de agregar.

### Archivos .cs → `<Compile Include="..." />`

```xml
<!-- En el grupo de Compile, ordenado con los demás Controllers/Models/etc. -->
<Compile Include="Controllers\{NombreDominio}Controller.cs" />
<Compile Include="Conversiones\Impl\Perfiles\{NombreDominio}MappingProfile.cs" />
<Compile Include="Models\{NombreDominio}Model.cs" />
```

### Archivos estáticos y vistas → `<Content Include="..." />`

```xml
<Content Include="Scripts\{nombreDominioCamel}.js" />
<Content Include="Styles\{nombreDominioCamel}.css" />
<Content Include="Styles\variables.css" />   <!-- si no existe ya -->

<Content Include="Views\{NombreDominio}\_CrearModificar.cshtml" />
<Content Include="Views\{NombreDominio}\Crear.cshtml" />
<Content Include="Views\{NombreDominio}\Index.cshtml" />
<Content Include="Views\{NombreDominio}\Listar.cshtml" />
<Content Include="Views\{NombreDominio}\Modificar.cshtml" />
```

### Cómo verificar

```powershell
Select-String "{NombreDominio}" "c:\Repositorios\MOA\Orquestador\Molinos.Orquest.Web\Molinos.Orquest.Web.csproj"
```

Si algún archivo no aparece en los resultados, agregarlo manualmente en el grupo XML correspondiente.

---

## Menú — _Menu.cshtml

Ubicación: `Molinos.Orquest.Web\Views\Shared\_Menu.cshtml`

Agregar dentro del submenú indicado en pre-flight usando el permiso y recurso correctos:

```cshtml
@if (PermisosHelper.Is(PermisosOrquestador.{NombrePermiso}))
{
    <li>@Html.ActionLink(Textos.{NombreDominio}_Administrar, "Index", "{NombreDominio}", null, new { @class = "dropdown-item" })</li>
}
```

### Submenús disponibles en el menú actual

| Submenu visible | Elemento padre en el HTML |
|-----------------|---------------------------|
| Concentrador | `dropdown-submenu` con texto `@Textos.Concentrador` |
| Grupos de dispositivos | `dropdown-submenu` con texto `@Textos.GrupoDispositivos_Titulo` |
| Nivel raíz (Administración) | Directamente en el `<ul class="dropdown-menu">` del nav-item Administración |

---

## Permiso — PermisosOrquestador.cs

Ubicación: `Molinos.Orquest.Dominio\Seguridad\PermisosOrquestador.cs`

```csharp
public const string {NombrePermiso} = "{NombrePermiso}";
// Si se separa lectura/escritura:
public const string {NombrePermiso}Editar = "{NombrePermiso}Editar";
```

---

## Database publish — Antes de compilar

Siempre que se agregue un nuevo permiso o una nueva tabla, informar al usuario:

> ⚠️ **Antes de compilar**, por favor realizá el **Publish** del proyecto `Molinos.Orquest.Database`  
> apuntando a tu instancia local de SQL Server para aplicar:
> - Las migraciones de nuevas tablas
> - Los registros de permisos en la tabla de seguridad
>
> Cuando esté listo, confirmá para continuar con el build.

No ejecutar MSBuild hasta recibir confirmación.

---

## Compilar y verificar

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  "c:\Repositorios\MOA\Orquestador\Molinos.Orquestador.sln" `
  /p:Configuration=Debug /nologo /v:m 2>&1 | Select-String " error "
```

- **Sin output** = build limpio ✅
- **Con output** = corregir errores antes de continuar ❌

### Compilar solo el proyecto Test

```powershell
& "C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  "c:\Repositorios\MOA\Orquestador\Molinos.Orquest.Test\Molinos.Orquest.Test.csproj" `
  /p:Configuration=Debug /nologo /v:m 2>&1 | Select-String " error "
```
