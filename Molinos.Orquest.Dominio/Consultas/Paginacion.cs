using System.Runtime.Serialization;
using Molinos.Orquest.Dominio.Consultas;

namespace Molinos.Scato.Dominio.Consultas
{
    [DataContract]
    public class Paginacion
    {
        [DataMember] 
        public string OrdenarPor { get; private set; }
        [DataMember] 
        public DirOrden DireccionOrden { get; private set; }
        [DataMember] 
        public int Pagina { get; private set; }
        [DataMember] 
        public int ItemsPorPagina { get; private set; }

        public Paginacion(
            string ordenarPor = null,
            DirOrden direccionOrden = DirOrden.Asc,
            int pagina = 1,
            int itemsPorPagina = 0)
        {
            OrdenarPor = ordenarPor;
            DireccionOrden = direccionOrden;
            Pagina = pagina;
            ItemsPorPagina = itemsPorPagina;
        }
    }
}
