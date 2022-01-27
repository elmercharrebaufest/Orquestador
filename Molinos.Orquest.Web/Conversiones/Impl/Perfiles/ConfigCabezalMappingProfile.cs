using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigCabezalMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigCabezalMappingProfile"; }
        }
        public ConfigCabezalMappingProfile()
        {
            CreateMap<ConfigCabezal, ConfigCabezalModel>();
            CreateMap<ConfigCabezalModel, ConfigCabezal>();
        }
    }
}
