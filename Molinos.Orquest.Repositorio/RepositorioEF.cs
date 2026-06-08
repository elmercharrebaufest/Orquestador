using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Scato.Dominio.Consultas;

namespace Molinos.Orquest.Repositorio
{
    public sealed class RepositorioEF : IRepositorio
    {
        private readonly DbContext context;
        private const int SqlFkError = 547;

        public RepositorioEF(DbContext context)
        { //Forzar el uso del Sql Provider para que la EntityFramework.SqlServer.dll se copie al proyecto web
            //http://robsneuron.blogspot.com/2013/11/entity-framework-upgrade-to-6.html
            var ensureDLLIsCopied = System.Data.Entity.SqlServer.SqlProviderServices.Instance;

            this.context = context;
        }

        private IDbSet<TEntidad> Set<TEntidad>() where TEntidad : class
        {
            return context.Set<TEntidad>();
        }

        public TEntidad Obtener<TEntidad>(object id) where TEntidad : class
        {
            return Set<TEntidad>().Find(id);
        }

        public TEntidad Obtener<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class
        {
            return Set<TEntidad>().SingleOrDefault(condicion);
        }
        public TEntidad ObtenerNoTracking<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class
        {
            return Set<TEntidad>().AsNoTracking().SingleOrDefault(condicion);
        }
        public TProyeccion Obtener<TEntidad, TProyeccion>(Expression<Func<TEntidad, bool>> condicion, Expression<Func<TEntidad, TProyeccion>> proyeccion) where TEntidad : class
        {
            return Set<TEntidad>().Where(condicion).Select(proyeccion).SingleOrDefault();
        }

        public IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, bool>> condicion = null) where TEntidad : class
        {
            IQueryable<TEntidad> resultado = Set<TEntidad>();
            if (condicion != null)
            {
                resultado = resultado.Where(condicion);
            }
            return resultado.ToList();
        }

        public IList<TResultado> Listar<TEntidad, TResultado>(Expression<Func<TEntidad, Boolean>> condicion, Expression<Func<TEntidad, TResultado>> seleccion) where TEntidad : class
        {
            return Set<TEntidad>().Where(condicion).Select(seleccion).ToList();
        }

        public int Contar<TEntidad>() where TEntidad : class
        {
            return Set<TEntidad>().Count();
        }

        public int Contar<TEntidad>(Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            return Set<TEntidad>().Count(filtro);
        }

        public bool Existe<TEntidad>(Expression<Func<TEntidad, bool>> filtro) where TEntidad : class
        {
            return Set<TEntidad>().Any(filtro);
        }

        public TEntidad Agregar<TEntidad>(TEntidad entidad) where TEntidad : class
        {
            return Set<TEntidad>().Add(entidad);
        }

        public TEntidad Remover<TEntidad>(object id) where TEntidad : class
        {
            return Remover(Obtener<TEntidad>(id));
        }

        public TEntidad Remover<TEntidad>(TEntidad entidad) where TEntidad : class
        {
            return Set<TEntidad>().Remove(entidad);
        }

        public int GuardarCambios()
        {
            try
            {
                return context.SaveChanges();
            }
            catch (DataException e)
            {
                if (ObtenerCodigoError(e) == SqlFkError)
                {
                    throw new EntidadReferenciadaException(string.Empty, e);
                }
                throw;
            }
        }

        public Orquestador OrquestadorQueTieneTomado(string codigoDispositivo)
        {
            return Set<Dispositivo>()
                .Where(d => d.Codigo == codigoDispositivo)
                .Select(d => d.TomadoPor)
                .SingleOrDefault();
        }

        public string ObtenerCodigoDispositivoConcentrador(string codigoDispositivo)
        {
            return Set<Dispositivo>()
                .Where(d => d.Codigo == codigoDispositivo && d.Activo && !d.EsConcentrador)
                .Select(d => d.Concentrador.Codigo).SingleOrDefault() ?? codigoDispositivo;
        }

        public IList<string> DispositivosConSuscripcionesVencidas(DateTime fechaVencimiento)
        {
            return Set<Suscripcion>()
                .Where(s => s.UltimaSuscripcion < fechaVencimiento && !s.Persistente)
                .Select(s => s.Dispositivo.Codigo)
                .Distinct()
                .ToList();
        }

        public bool TomarDispositivo(int idOrquestador, string codigoDispositivo)
        {
            var count = 1;
            const int maxTries = 2;
            while (true)
            {
                try
                {
                    return context.Database.ExecuteSqlCommand(
                    @"UPDATE Dispositivo SET TomadoPor_Id = {0} 
                    WHERE (Codigo = {1} AND Activo = 1 AND (TomadoPor_Id IS NULL OR TomadoPor_Id = {0}))
                        OR Concentrador_Id IN 
                            (SELECT Id FROM Dispositivo WHERE Codigo = {1} AND Activo = 1 AND (TomadoPor_Id IS NULL OR TomadoPor_Id = {0}))",
                    idOrquestador,
                    codigoDispositivo) > 0;
                }
                catch
                {
                    Thread.Sleep(count * 500);
                    if (++count == maxTries)
                    {
                        return false;
                    }
                }
            }            
        }

        public bool LiberarDispositivo(int idOrquestador, string codigoDispositivo)
        {
            return context.Database.ExecuteSqlCommand(
                    @"UPDATE Dispositivo SET TomadoPor_Id = NULL 
                    WHERE (Codigo = {1} AND Activo = 1 AND TomadoPor_Id = {0})
                        OR Concentrador_Id IN 
                            (SELECT Id FROM Dispositivo WHERE Codigo = {1} AND Activo = 1 AND TomadoPor_Id = {0})",
                    idOrquestador, codigoDispositivo) > 0;
        }

        public void LiberarDispositivos(int idOrquestador)
        {
            context.Database.ExecuteSqlCommand(
                     "UPDATE Dispositivo SET TomadoPor_Id = NULL WHERE TomadoPor_Id = {0}", idOrquestador);
        }

        public bool TomarCIV(int idOrquestador, string codigoCIV)
        {
            return context.Database.ExecuteSqlCommand(
                @"UPDATE ConfigIdentificacionVehicular SET TomadoPor_Id = {0} WHERE Codigo = {1} AND TomadoPor_Id IS NULL",
                idOrquestador, codigoCIV) > 0;
        }

        public void LiberarCIV(int idOrquestador, string codigoCIV)
        {
            context.Database.ExecuteSqlCommand(
                "UPDATE ConfigIdentificacionVehicular SET TomadoPor_Id = NULL WHERE TomadoPor_Id = {0} AND Codigo = {1}",
                idOrquestador, codigoCIV);
        }

        public void LiberarCIVs(int idOrquestador)
        {
            context.Database.ExecuteSqlCommand(
                "UPDATE ConfigIdentificacionVehicular SET TomadoPor_Id = NULL WHERE TomadoPor_Id = {0}",
                idOrquestador);
        }

        public void Dispose()
        {
            context.Dispose();
        }

        private int ObtenerCodigoError(DataException e)
        {
            var code = 0;
            if (e.InnerException != null)
            {
                var sqlEx = e.InnerException.InnerException as SqlException;
                if (sqlEx != null)
                {
                    code = sqlEx.Number;
                }
            }
            return code;
        }

        public ListaPaginada<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, bool>> condicion, Paginacion paginacion) where TEntidad : class
        {
            IQueryable<TEntidad> resultados = Set<TEntidad>();
            if (condicion != null)
            {
                resultados = resultados.Where(condicion);
            }

            int itemsTotales = resultados.Count();

            if (paginacion.OrdenarPor != null)
            {
                var selectorOrden = Expresiones.Propiedad<TEntidad>(paginacion.OrdenarPor);
                resultados = paginacion.DireccionOrden == DirOrden.Asc
                                 ? resultados.OrderBy(selectorOrden)
                                 : resultados.OrderByDescending(selectorOrden);
            }

            resultados = resultados.Skip((paginacion.Pagina - 1) * paginacion.ItemsPorPagina).Take(paginacion.ItemsPorPagina);

            return new ListaPaginada<TEntidad>(resultados.ToList(), paginacion.Pagina, paginacion.ItemsPorPagina, itemsTotales);
        }

    }
}
