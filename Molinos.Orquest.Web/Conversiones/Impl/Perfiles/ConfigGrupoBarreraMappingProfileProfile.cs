using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigGrupoBarreraMappingProfileProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigGrupoBarreraMappingProfileProfile"; }
        }
        public ConfigGrupoBarreraMappingProfileProfile()
        {
            CreateMap<ConfigGrupoBarrera, ConfigGrupoBarreraModel>();
            CreateMap<ConfigGrupoBarreraModel, ConfigGrupoBarrera>();
        }
    }
}
