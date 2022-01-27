using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public abstract class ProcesadorDispositivo : IProcesadorDispositivo
    {
        protected readonly IAdministradorSuscripciones AdminSuscripciones;
        protected readonly IProcesadorFactory ProcesadorFactory;
        protected readonly ILogger Log;
        
        private readonly ConcurrentQueue<ComandoAsincronico> comandos = new ConcurrentQueue<ComandoAsincronico>();

        protected readonly IDriver Driver;
        protected readonly Dispositivo Dispositivo;

        private readonly Action<IProcesadorDispositivo> callBackFinProcesamiento;

        private Action accionPostComandoEncolado;

        public string CodigoDispositivo { get { return Dispositivo.Codigo; } }

        protected ProcesadorDispositivo(Dispositivo dispositivo, IProcesadorFactory procesadorFactory, IDriverFactory driverFactory, IAdministradorSuscripciones adminSuscripciones, Action<IProcesadorDispositivo> callBackFinProcesamiento, ILogger log)
        {
            ProcesadorFactory = procesadorFactory;
            this.callBackFinProcesamiento = callBackFinProcesamiento;
            Log = log;
            AdminSuscripciones = adminSuscripciones;
            Dispositivo = dispositivo;

            log.Debug("Instanciando el driver para el dispositivo {0}", dispositivo.Codigo);
            Driver = driverFactory.Driver<IDriver>(dispositivo);

            //Al encolar el primer comando, iniciar el ciclo de procesamiento
            accionPostComandoEncolado = Iniciar;
        }

        private void Iniciar()
        {
            // Una vez iniciado el ciclo de procesamiento, no se debe iniciar nada al encolar un comando
            accionPostComandoEncolado = () => { };

            Log.Debug("Iniciando cola de procesamiento para el dispositivo {0}", CodigoDispositivo);
            Task.Run(() =>
                {
                    Log.Debug("Iniciando ciclo de procesamiento de comandos");
                    ProcesarComandos();
                    Log.Debug("Cola de comandos vacía. Terminando procesamiento");
                    callBackFinProcesamiento(this);
                });
        }

        private void ProcesarComandos()
        {
            ComandoAsincronico comandoAsinc;
            while (comandos.TryDequeue(out comandoAsinc))
            {
                try
                {
                    Log.Debug("Desencolando comando: {0}", comandoAsinc.Comando);
                    comandoAsinc.Resultado = ProcesarComando(comandoAsinc.Comando);
                }
                catch (Exception e)
                {
                    comandoAsinc.Exception = e;
                }
                finally
                {
                    comandoAsinc.EventoFin.Set();
                }
            }
        }

        protected abstract ResultadoComando ProcesarComando(Comando comando);


        public ResultadoComando Procesar(Comando comando)
        {
            Log.Debug("Encolando comando {0}", comando);
            var comandoAsincronico = new ComandoAsincronico(comando);
            comandos.Enqueue(comandoAsincronico);
            
            accionPostComandoEncolado();
           
            Log.Debug("Esperando procesamiento de comando {0}", comando);
            comandoAsincronico.EventoFin.WaitOne();

            if (comandoAsincronico.Exception != null)
            {
                Log.Debug("La ejecución del comando retornó una excepcion. Re-lanzandola...");
                throw comandoAsincronico.Exception;
            }
            Log.Debug("Comando {0} procesado", comando);
            return comandoAsincronico.Resultado;
        }

        public bool ProcesarRemanentes()
        {
            // Procesa comandos que se hayan encolado luego de que termino el ciclo y antes de llamar este método
            ProcesarComandos();
            accionPostComandoEncolado = Iniciar;
            var puedeLiberar = PuedeLiberarProcesador();
            Log.Debug("Procesamiento de la cola para el dispositivo {0} finalizado", CodigoDispositivo);
            return puedeLiberar;
        }

        protected abstract bool PuedeLiberarProcesador();

        private class ComandoAsincronico
        {
            public Comando Comando { get; private set; }
            public EventWaitHandle EventoFin { get; private set; }

            public ResultadoComando Resultado { get; set; }

            public Exception Exception { get; set; }

            public ComandoAsincronico(Comando comando)
            {
                Comando = comando;
                EventoFin = new ManualResetEvent(false);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                accionPostComandoEncolado = () =>
                {
                    throw new ProcesadorException("El procesador no esta en un estado que pueda aceptar comandos");
                };
                var disposable = Driver as IDisposable;
                if (disposable != null)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Log.Debug(ex, ex.Message);
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "El warning es por la linea de logging")]
        public void Dispose()
        {
            Log.Debug("Procesamiento de la cola para el dispositivo {0} finalizado", CodigoDispositivo);
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
