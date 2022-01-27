using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigItcMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigItcMappingProfile"; }
        }
        public ConfigItcMappingProfile()
        {
            CreateMap<ConfigItc, ConfigItcModel>();
            CreateMap<ConfigItcModel, ConfigItc>();
        }
    }
}
