using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class ConfigIdentificacionVehicularMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "ConfigIdentificacionVehicularMappingProfile"; }
        }

        public ConfigIdentificacionVehicularMappingProfile()
        {
            CreateMap<ConfigIdentificacionVehicular, ConfigIdentificacionVehicularModel>()
                .ForMember(dest => dest.Camaras, opt => opt.MapFrom(src => src.Camaras));

            CreateMap<ConfigIdentificacionVehicularCamara, ConfigIdentificacionVehicularModel.CamaraItemModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ConfigCamaraId))
                .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src =>
                    src.ConfigCamara != null ? src.ConfigCamara.Dispositivo.Descripcion : src.ConfigCamaraId.ToString()))
                .ForMember(dest => dest.Ip, opt => opt.MapFrom(src =>
                    src.ConfigCamara != null ? src.ConfigCamara.Uri : string.Empty));

            CreateMap<ConfigIdentificacionVehicularModel, ConfigIdentificacionVehicular>()
                .ForMember(dest => dest.Camaras, opt => opt.Ignore());
        }
    }
}
