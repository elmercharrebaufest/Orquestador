using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Consultas
{
    [DataContract]
    public class ListaPaginada<TEntidad> : IEnumerable<TEntidad>
    {
        [DataMember]
        public int Pagina { get; private set; }
        [DataMember]
        public int ItemsPorPagina { get; private set; }
        [DataMember]
        public int ItemsTotales { get; private set; }
        [DataMember]
        public IList<TEntidad> Items { get; private set; }

        public ListaPaginada(IList<TEntidad> items, int pagina, int itemsPorPagina, int itemsTotales)
        {
            Items = items;
            Pagina = pagina;
            ItemsPorPagina = itemsPorPagina;
            ItemsTotales = itemsPorPagina == 0 ? items.Count : itemsTotales;
        }

        public IEnumerator<TEntidad> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
