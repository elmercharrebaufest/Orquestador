using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Scato.Dominio.Consultas;

namespace Molinos.Orquest.Repositorio
{
    /// <summary>
    /// Representa el comportamiento de un repositorio
    /// </summary>
    public interface IRepositorio : IDisposable
    {
        /// <summary>
        /// Devuelve una entidad dada su clave primaria
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="id">clave primaria de la entidad</param>
        /// <returns>La entidad que posee esa clave o null si no se encuentra ninguna</returns>
        TEntidad Obtener<TEntidad>(object id) where TEntidad : class;

        /// <summary>
        /// Devuelve una unica entidad que cumple con la condición
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe devolverse o no</param>
        /// <returns>La unica entidad que se corresponde con la condicion o null si no la encuentra</returns>
        /// <exception cref="InvalidOperationException">Se lanza cuando más de una entidad comple con la condición</exception>
        TEntidad Obtener<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;
        TEntidad ObtenerNoTracking<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;
        TProyeccion Obtener<TEntidad, TProyeccion>(Expression<Func<TEntidad, bool>> condicion, Expression<Func<TEntidad, TProyeccion>> proyeccion) where TEntidad : class;

        /// <summary>
        /// Lista todas las entidades que complen con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a devolver</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe devolverse o no</param>
        /// <returns>Todas las entidades que cumplen con la condicion</returns>
        IList<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion = null) where TEntidad : class;

        IList<TResultado> Listar<TEntidad, TResultado>(Expression<Func<TEntidad, Boolean>> condicion, Expression<Func<TEntidad, TResultado>> seleccion) where TEntidad : class;

        ListaPaginada<TEntidad> Listar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion, Paginacion paginacion) where TEntidad : class;

        /// <summary>
        /// Devuelve la cantidad de entidades en el repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a contar</typeparam>
        /// <returns>La cantidad de entidades de un tipo determinado en el repositorio</returns>
        int Contar<TEntidad>() where TEntidad : class;

        /// <summary>
        /// Devuelve la cantidad de entidades en el repositorio que cumplen con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a contar</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe contarse o no</param>
        /// <returns>La cantidad de entidades de un tipo determinado en el repositorio</returns>
        int Contar<TEntidad>(Expression<Func<TEntidad, Boolean>> condicion) where TEntidad : class;

        /// <summary>
        /// Devuelve si existe una entidad que cumpla con la condicion
        /// </summary>
        /// <typeparam name="TEntidad">Tipo entidades a comprobar</typeparam>
        /// <param name="condicion">Expresion que dada una entidad devuelve si debe contarse o no</param>
        /// <returns>true si existe alguna entidad que compla con la condición</returns>
        bool Existe<TEntidad>(Expression<Func<TEntidad, bool>> condicion) where TEntidad : class;

        /// <summary>
        /// Agrega una entidad al repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a agregar</typeparam>
        /// <param name="entidad">Entidad a agregar</param>
        /// <returns>Entidad agregada</returns>
        TEntidad Agregar<TEntidad>(TEntidad entidad) where TEntidad : class;

        /// <summary>
        /// Remueve una entidad del repositorio
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a remover</typeparam>
        /// <param name="entidad">Entidad a remover</param>
        /// <returns>Entidad removida</returns>
        TEntidad Remover<TEntidad>(TEntidad entidad) where TEntidad : class;

        /// <summary>
        /// Remueve una entidad del repositorio dada su clave primaria
        /// </summary>
        /// <typeparam name="TEntidad">Tipo de la entidad a remover</typeparam>
        /// <param name="id">clave de la entidad a remover</param>
        /// <returns>Entidad removida</returns>
        TEntidad Remover<TEntidad>(object id) where TEntidad : class;

        /// <summary>
        /// Persiste los cambios hechos al repositorio
        /// </summary>
        /// <returns>Cantidad de entidades agregadas/actualizadas</returns>
        int GuardarCambios();

        Orquestador OrquestadorQueTieneTomado(string codigoDispositivo);
        IList<String> DispositivosConSuscripcionesVencidas(DateTime fechaVencimiento);

        bool TomarDispositivo(int idOrquestador, string codigoDispositivo);
        bool LiberarDispositivo(int idOrquestador, string codigoDispositivo);
        void LiberarDispositivos(int idOrquestador);
        string ObtenerCodigoDispositivoConcentrador(string codigoDispositivo);
    }
}
