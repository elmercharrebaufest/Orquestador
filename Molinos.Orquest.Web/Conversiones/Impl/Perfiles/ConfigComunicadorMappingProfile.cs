using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigComunicadorMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigComunicadorMappingProfile"; }
        }
        public ConfigComunicadorMappingProfile()
        {
            CreateMap<ConfigComunicador, ConfigComunicadorModel>();
            CreateMap<ConfigComunicadorModel, ConfigComunicador>();
        }
    }
}
