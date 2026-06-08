# Referencia: Model y AutoMapper Profile

## Model — {NombreDominio}Model.cs

```csharp
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Web.Models
{
    public class {NombreDominio}Model
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "{NombreDominio}_Nombre", ResourceType = typeof(Textos))]
        public string Nombre { get; set; }

        [Required]
        [Display(Name = "{NombreDominio}_Codigo", ResourceType = typeof(Textos))]
        public string Codigo { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        // Navegación opcional — nullable para campos no requeridos
        public int? OtraEntidadId { get; set; }

        // Colección de sub-entidades (si aplica)
        public IList<ItemAnidadoModel> Hijos { get; set; } = new List<ItemAnidadoModel>();

        // Clase anidada para sub-entidades serializable al cliente
        // Usar PascalCase español para que el JSON coincida con el servidor
        public class ItemAnidadoModel
        {
            public int    Id     { get; set; }
            public string Nombre { get; set; }
            public string Ip     { get; set; }
        }
    }
}
```

---

## AutoMapper Profile — {NombreDominio}MappingProfile.cs

```csharp
using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class {NombreDominio}MappingProfile : Profile
    {
        protected override void Configure()
        {
            // Entidad → Model (para GET Modificar)
            Mapper.CreateMap<{EntidadDominio}, {NombreDominio}Model>();

            // Sub-entidad → Model anidado (si aplica)
            Mapper.CreateMap<{EntidadHija}, {NombreDominio}Model.ItemAnidadoModel>()
                .ForMember(dest => dest.Id,     opt => opt.MapFrom(src => src.SubEntidadId))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.SubEntidad.Dispositivo.Descripcion))
                .ForMember(dest => dest.Ip,     opt => opt.MapFrom(src => src.SubEntidad.Uri));
        }
    }
}
```

### Notas

- Los profiles se cargan por **reflexión** en el startup — no requieren registro manual, solo deben estar en el namespace `Molinos.Orquest.Web.Conversiones.Impl.Perfiles` y heredar de `Profile`
- Si AutoMapper mapea automáticamente propiedades con el mismo nombre, no hace falta `ForMember`
- Para propiedades que NO deben mapearse: `opt => opt.Ignore()`
- La clase anidada `ItemAnidadoModel` usa PascalCase español (`Id, Nombre, Ip`) para que al serializar con `JsonConvert` los nombres coincidan con lo que consume el JS
