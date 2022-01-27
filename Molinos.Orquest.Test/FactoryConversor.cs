using Molinos.Orquest.Web.Conversiones.Impl;

namespace Molinos.Orquest.Test
{
    public class FactoryConversor
    {
        private static readonly ConversorAutoMapper conversor = new ConversorAutoMapper();

        public static ConversorAutoMapper ConversorAutoMapper
        {
            get { return conversor; }
        }
    }
}
