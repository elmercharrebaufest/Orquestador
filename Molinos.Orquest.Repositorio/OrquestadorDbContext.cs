using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Molinos.Orquest.Dominio.Entidades;

namespace Molinos.Orquest.Repositorio
{

    public class OrquestadorDbContext : DbContext
    {
        public OrquestadorDbContext()
            : base("OrquestadorDb")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // No usamos nombres de tablas en plural. Igualmente la pluralización fucniona solo con nombres en ingles.
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            //Se mapean todas las entidades bajo el namespace Molinos.Scato.Dominio.Entidades      
            MapearAssemblyDe<Dispositivo>(modelBuilder, 
                incluir: x => x.Namespace == typeof(Dispositivo).Namespace,
                excluir: null);



        }

        private void MapearAssemblyDe<TEntidad>(DbModelBuilder modelBuilder, Predicate<Type> incluir, Predicate<Type> excluir)
        {
            var tiposEntidades = typeof(TEntidad).Assembly.GetTypes()
                .Where(x => incluir(x) && !x.IsNestedPrivate);
            if (excluir != null)
            {
                tiposEntidades = tiposEntidades.Where(x => !excluir(x));
            }

            var metodo = modelBuilder.GetType().GetMethod("Entity");
            foreach (var tipoEntidad in tiposEntidades)
            {
                metodo.MakeGenericMethod(tipoEntidad).Invoke(modelBuilder, null);
            }
        }
    }


}
