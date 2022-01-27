using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Resultados
{
    [DataContract]
    [KnownType("TiposDeResultadoComandos")]
    public class ResultadoComando
    {
        [DataMember]
        public Mensaje Mensaje { get; set; }

        /// <summary>
        /// Este metodo es para que cuando se exponga la clase Comando por WCF, se expongan tambien todas las subclases
        /// </summary>
        /// <returns>La lista de subclases de ResultadoComando que hay en el assembly</returns>
        public static Type[] TiposDeResultadoComandos()
        {
            var tipoComando = typeof(ResultadoComando);
            return tipoComando.Assembly.GetTypes().Where(tipoComando.IsAssignableFrom).ToArray();
        }

    }
}
