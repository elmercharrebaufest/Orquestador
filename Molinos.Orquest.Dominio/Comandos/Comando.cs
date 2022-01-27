using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Molinos.Orquest.Dominio.Comandos
{

    [DataContract]
    [KnownType("TiposDeComandos")]
    public abstract class Comando
    {
        private string codigoDispositivo;
        [DataMember]
        public string CodigoDispositivo
        {
            get { return codigoDispositivo; }
            set { codigoDispositivo = (value != null) ? value.ToUpper() : null; }
        }


        /// <summary>
        /// Este metodo es para que cuando se exponga la clase Comando por WCF, se expongan tambien todas las subclases
        /// </summary>
        /// <returns>La lista de subclases de Comando que hay en el assembly</returns>
        public static Type[] TiposDeComandos()
        {
            var tipoComando = typeof(Comando);
            return tipoComando.Assembly.GetTypes().Where(tipoComando.IsAssignableFrom).ToArray();
        }
    }
}
