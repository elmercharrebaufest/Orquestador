using Molinos.Orquest.Dominio.Entidades;

namespace Molinos.Orquest.Dominio.Dtos
{
    public class DispositivoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public string ClaseDriver { get; set; }
        public string Sector { get; set; }
        public bool Activo { get; set; }
        public bool EstadoCorrecto { get; set; }
        public bool EsConcentrador { get; set; }
    }
}
