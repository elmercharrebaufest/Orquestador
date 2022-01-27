using System.Collections.Generic;
using Molinos.Orquest.Dominio.Consultas;

namespace Molinos.Orquest.Web.Conversiones
{
    public interface IConversor
    {
        TSalida Convertir<TEntrada, TSalida>(TEntrada entrada);
        TSalida Convertir<TEntrada, TSalida>(TEntrada entrada, TSalida salida);
        ListaPaginada<TSalida> ConvertirListaPaginada<TEntrada, TSalida>(ListaPaginada<TEntrada> lista);
        IList<TSalida> ConvertirList<TEntrada, TSalida>(IList<TEntrada> lista);
    }
}
