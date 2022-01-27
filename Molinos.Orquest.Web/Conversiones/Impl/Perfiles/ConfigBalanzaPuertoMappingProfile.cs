using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigBalanzaPuertoMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigBalanzaPuertoMappingProfile"; }
        }
        public ConfigBalanzaPuertoMappingProfile()
        {
            CreateMap<ConfigBalanzaPuerto, ConfigBalanzaPuertoModel>();
            CreateMap<ConfigBalanzaPuertoModel, ConfigBalanzaPuerto>();
        }
    }
}
