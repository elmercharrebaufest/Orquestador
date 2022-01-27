using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigNirsMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigNirsMappingProfile"; }
        }
        public ConfigNirsMappingProfile()
        {
            CreateMap<ConfigNirs, ConfigNirsModel>();
            CreateMap<ConfigNirsModel, ConfigNirs>();
        }
    }
}
