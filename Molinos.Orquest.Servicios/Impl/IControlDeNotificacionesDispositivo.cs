using System;

namespace Molinos.Orquest.Servicios.Impl
{
    public interface IControlDeNotificacionesDispositivo
    {

        /// <summary>
        /// Incrementa el contador y devuelve la cantidad cuantificada cuando excede el maximo
        /// </summary>
        bool NewValue();

        /// <summary>
        /// Inicia un contandor y acumula todas las peticiones sin hacer reinicio
        /// </summary>
        long TotalCounter { get; }

        /// <summary>
        /// Captura una marca de tiempo cuando inicia el contador
        /// </summary>
        DateTime TotalInit { get; }
    }
} 