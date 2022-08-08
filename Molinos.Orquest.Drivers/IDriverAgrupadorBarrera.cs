using Molinos.Orquest.Dominio.Resultados;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Drivers
{

    public interface IDriverAgrupadorBarrera : IDriver
    {
        void Abrir();
        void Cerrar();
        ResultadoEstadoSensor ConsultaEstadoActual();
        void NotificarEstadoActualSensor();
    }
}
