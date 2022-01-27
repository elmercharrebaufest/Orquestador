using System;
using System.Configuration;
using System.Threading;
using Molinos.Orquest.Dominio;

namespace Molinos.Orquest.Servicios.Impl
{
    public sealed class ProgramadorTareas : IProgramadorTareas, IDisposable
    {
        private Timer timer;
        private Timer timerEstado;

        public event EventHandler<DepurarSuscripcionesEventArgs> DepurarSuscripciones;
        public event EventHandler<ActualizarEstadoEventArgs> ActualizarEstado;

        public void Iniciar()
        {
            var intervalo = int.Parse(ConfigurationManager.AppSettings["IntervaloDepuracionSuscripciones"]);
            var intervaloEstado = int.Parse(ConfigurationManager.AppSettings["IntervaloActualizarEstado"]);

            
            timer = new Timer(
                obj => OnDepurarSuscripciones(),
                null,
                1000,
                intervalo*1000);

            if(intervaloEstado > 0)
            {
                timerEstado = new Timer(
                obj => OnActualizarEstado(),
                null,
                1000,
                intervaloEstado * 1000);
            }            
        }

        private void OnActualizarEstado()
        {
            EventHandler<ActualizarEstadoEventArgs> handler = ActualizarEstado;
            if (handler != null)
            {
                handler(this, new ActualizarEstadoEventArgs ());
            }

        }

        private void OnDepurarSuscripciones()
        {
            var timeout = int.Parse(ConfigurationManager.AppSettings["TimeoutSuscripciones"]);
            var fechaTimeout = DateTime.Now.AddSeconds(-timeout);
            
            EventHandler<DepurarSuscripcionesEventArgs> handler = DepurarSuscripciones;
            if (handler != null)
            {
                handler(this, new DepurarSuscripcionesEventArgs {Vencimiento = fechaTimeout});
            }

        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Dispose();
            }
        }

    }
}
