using Molinos.Orquest.Dominio.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class DatosPersonalesQrDto
    {
        public string Dni { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public TipoQr? TipoDeQr { get; set; }
        public string Cuit { get; set; }
    }
}
