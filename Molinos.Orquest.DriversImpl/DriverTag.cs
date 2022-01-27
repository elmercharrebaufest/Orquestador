using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverTag : DriverBase, IDriverTag, IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigTag configTag;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigTag); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configTag = (ConfigTag)configuracion;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public ResultadoEjecutarQuery EjecutarQuery()
        {
            return ((DriverHistorian)driverItc).ObtenerDatos(configTag.Query, configTag.Dispositivo.Codigo);
        }

        public ResultadoEjecutarQuery EjecutarQuery(string query)
        {
            return ((DriverHistorian)driverItc).ObtenerDatos(query, configTag.Dispositivo.Codigo);
        }

        public IDriver DriverFisico
        {
            set { driverItc = (IDriverItc)value; }
        }

    }
}
