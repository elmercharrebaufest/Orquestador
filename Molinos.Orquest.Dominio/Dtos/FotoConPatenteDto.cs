using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class FotoConPatentesDto
    {
        public Dictionary<string, float> Patentes { get; set; }
        public byte[] Imagen { get; set; }
    }
}
