using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigHumedimetroMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigHumedimetroMappingProfile"; }
        }
        public ConfigHumedimetroMappingProfile()
        {
           CreateMap<ConfigHumedimetro, ConfigHumedimetroModel>();
           CreateMap<ConfigHumedimetroModel, ConfigHumedimetro>();
        }
    }
}
