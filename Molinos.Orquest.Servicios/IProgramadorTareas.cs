using System;
using Molinos.Orquest.Dominio;

namespace Molinos.Orquest.Servicios
{
    public interface IProgramadorTareas
    {
        event EventHandler<DepurarSuscripcionesEventArgs> DepurarSuscripciones;
        event EventHandler<ActualizarEstadoEventArgs> ActualizarEstado;

        void Iniciar();
    }
}
