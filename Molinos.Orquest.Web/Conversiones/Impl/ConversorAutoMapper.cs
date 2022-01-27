using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Web.Conversiones.Impl.Perfiles;

namespace Molinos.Orquest.Web.Conversiones.Impl
{
    public class ConversorAutoMapper : IConversor
    {
        public ConversorAutoMapper()
        {
            Configurar();
        }

        private static void Configurar()
        {
            Mapper.Initialize(x =>
            {
                // Por reflection se agregan todas las clases que esten en este assembly y hereden de Profile   

                var perfiles = typeof(ConfigCabezalMappingProfile)
                    .Assembly.GetTypes().Where(t => t.BaseType == typeof(Profile));

                var metodo = typeof(IMapperConfigurationExpression).GetMethod("AddProfile", new Type[0]);

                foreach (var tipoPerfil in perfiles)
                {
                    metodo.MakeGenericMethod(tipoPerfil).Invoke(x, null);
                }
            });
        }

        public TSalida Convertir<TEntrada, TSalida>(TEntrada entrada)
        {
            return Mapper.Map<TEntrada, TSalida>(entrada);
        }

        public TSalida Convertir<TEntrada, TSalida>(TEntrada entrada, TSalida salida)
        {
            return Mapper.Map(entrada, salida);
        }

        public ListaPaginada<TSalida> ConvertirListaPaginada<TEntrada, TSalida>(ListaPaginada<TEntrada> lista)
        {
            return new ListaPaginada<TSalida>(
                lista.Items.Select(Convertir<TEntrada, TSalida>).ToList(),
                lista.Pagina,
                lista.ItemsPorPagina,
                lista.ItemsTotales);
        }

        public IList<TSalida> ConvertirList<TEntrada, TSalida>(IList<TEntrada> lista)
        {
            return lista.Select(Convertir<TEntrada, TSalida>).ToList();
        }
    }
}
