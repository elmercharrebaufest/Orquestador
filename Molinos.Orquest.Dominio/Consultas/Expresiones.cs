using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Molinos.Orquest.Dominio.Consultas
{
    public static class Expresiones
    {
        public static Expression<Func<TEntidad, object>> Propiedad<TEntidad>(string propiedad)
        {
            var propiedades = propiedad.Split('.');
            var tipo = typeof(TEntidad);
            var parametro = Expression.Parameter(tipo, "x");
            Expression expresion = parametro;
            PropertyInfo infoPropiedad = null;
            foreach (var nombrePropiedad in propiedades)
            {
                infoPropiedad = tipo.GetProperty(nombrePropiedad);
                expresion = Expression.Property(expresion, infoPropiedad);
                tipo = infoPropiedad.PropertyType;
            }

            if (infoPropiedad != null && infoPropiedad.PropertyType.IsValueType)
            {
                // Si la propiedad devuelve un value type hay que wrapearla en una clase.
                // Lo correcto seria hace rel boxing con un convert pero Linq to Entities no lo soporta.
                var wrapperType = typeof(Wrapper<>).MakeGenericType(infoPropiedad.PropertyType);
                expresion = Expression.MemberInit(
                        Expression.New(wrapperType),
                        Expression.Bind(wrapperType.GetProperty("Value"), expresion));


            }
            return Expression.Lambda<Func<TEntidad, object>>(expresion, parametro);
        }

        public static Expression<Func<TEntidad, object>> PropiedadValueType<TEntidad>(string propiedad)
        {
            var propiedades = propiedad.Split('.');
            var tipo = typeof(TEntidad);
            var parametro = Expression.Parameter(tipo, "x");
            Expression expresion = parametro;
            PropertyInfo infoPropiedad = null;
            foreach (var nombrePropiedad in propiedades)
            {
                infoPropiedad = tipo.GetProperty(nombrePropiedad);
                expresion = Expression.Property(expresion, infoPropiedad);
                tipo = infoPropiedad.PropertyType;
            }

            if (infoPropiedad != null && infoPropiedad.PropertyType.IsValueType)
            {
                expresion = Expression.TypeAs(expresion, typeof(object));
            }
            return Expression.Lambda<Func<TEntidad, object>>(expresion, parametro);
        }
    }

    internal class Wrapper<T>
    {
        public T Value { get; set; }
    }
}
