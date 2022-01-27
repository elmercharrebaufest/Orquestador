using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class RolMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "RolMappingProfile"; }
        }
        public RolMappingProfile()
        {
            CreateMap<Rol, RolModel>();
            CreateMap<RolModel, Rol>();
        }
    }
}
