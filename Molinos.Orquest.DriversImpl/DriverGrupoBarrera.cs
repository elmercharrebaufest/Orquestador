using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverGrupoBarrera : DriverBase, IDriverAgrupadorBarrera
    {
        public override Type TipoDispositivo => throw new NotImplementedException();

        public void Abrir()
        {
            throw new NotImplementedException();
        }

        public void Cerrar()
        {
            throw new NotImplementedException();
        }

        public ResultadoEstadoSensor ConsultaEstadoActual()
        {
            throw new NotImplementedException();
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            throw new NotImplementedException();
        }

        public void NotificarEstadoActualSensor()
        {
            throw new NotImplementedException();
        }

        public override void VerificarDispositivo()
        {
            throw new NotImplementedException();
        }
    }
}
