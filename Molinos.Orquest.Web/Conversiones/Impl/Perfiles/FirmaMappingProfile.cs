using AutoMapper;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Web.Models;

namespace Molinos.Orquest.Web.Conversiones.Impl.Perfiles
{
    public class FirmaMappingProfile : Profile
    {
        public override string ProfileName
        {
            get { return "FirmaMappingProfile"; }
        }
        public FirmaMappingProfile()
        {
           CreateMap<Firma, FirmaModel>();
           CreateMap<FirmaModel, Firma>();
        }
    }
}
