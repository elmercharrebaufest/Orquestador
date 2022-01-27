using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Dominio.Comandos
{
   public class EjecutarEjecutarQuery : ComandoEjecutar
    {
        public string CustomQuery { get; set; }
    }
}
