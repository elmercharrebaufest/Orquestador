using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class MeteorologicaDto
    {
        public string Descripcion { get; set; }
        public List<string> Detalle { get; set; }
        public List<byte[]> Imagenes { get; set; }
    }
}
