using System;
using System.Globalization;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroDummy : DriverBase, IDriverHumedimetro
    {

        private string codigoHumedimetro;
        private ConfigHumedimetro configHumedimetro;
        public override Type TipoDispositivo
        {
            get { return typeof (ConfigHumedimetro); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
        }

        public override void VerificarDispositivo()
        {
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            string[] frase = new string[] { "", "10/03/22", "01:57:57", "11.7", "70.6", "21.5", "24.2", "22.6", "SOJA ARG", "S/N: 1716-32552", "2", "785", "2301", "215", "070815" };
            var arrayHumedad = frase;
            if (arrayHumedad.Length != 15)
            {
                throw new FormatException(frase.ToString());
            }
            var stringHumedad = arrayHumedad[3];
            return decimal.Parse(stringHumedad, CultureInfo.InvariantCulture);

        }

        public decimal? ObtenerPH(DateTime? fechaDeInicio = null)
        {
            Log.Info("Captura de PH - Metodo ObtenerPH - DriverHumedimetroDummy");
            string[] frase = {"", "10/03/22", "01:57:57", "11.7", "70.6", "21.5", "24.2", "22.6", "SOJA ARG", "S/N: 1716-32552", "2", "785", "2301", "215", "070815" };
            var arrayPH = frase;
            if (arrayPH.Length != 15)
            {
                throw new FormatException(frase.ToString());
            }
            var stringPH= arrayPH[4];
            return decimal.Parse(stringPH, CultureInfo.InvariantCulture);

        }
    }
}
