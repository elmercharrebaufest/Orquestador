using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Dominio.Dtos
{

    public class EstadoMolinete
    {
        public ConsultaEstados ConsultaEstados { get; set; }
    }
    public class ConsultaEstados
    {
        public bool EstadoGeneral { get; set; }
    }

}
