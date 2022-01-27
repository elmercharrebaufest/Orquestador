using System;
using System.Collections.Generic;
using System.Linq;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Servicios.Procesamiento;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Parameters;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ProcesadorFactory : IProcesadorFactory
    {
        private readonly IKernel kernel;
        private IDictionary<Type, Type> procesadores;
        private ILogger log;
        
        public ProcesadorFactory(IKernel kernel, ILogger log)
        {
            this.kernel = kernel;
            this.log = log;
            RegistrarProcesadores();
        }
        
        private void RegistrarProcesadores()
        {
            log.Debug("Registrando procesadores de comandos.");
            procesadores = new Dictionary<Type, Type>();

            log.Debug("Obteniendo lista de comandos.");
            var comandos = Comando.TiposDeComandos().Where(t => !t.IsAbstract);
            log.Debug("Se obtuvo la lista de comandos.");
            foreach (var comando in comandos)
            {
                log.Debug("Obteniendo procesador para comando: {0}", comando.Name);
                procesadores.Add(comando, ObtenerProcesador(comando));
            }
            log.Debug("Se registraron {0} procesadores de comandos.", procesadores.Count);
        }

        private Type ObtenerProcesador(Type comando)
        {
            log.Debug("Obteniendo Procesadores.");
            return typeof (IProcesadorComando<,>)
                .Assembly
                .GetExportedTypes()
                .Single(
                    x => !x.IsAbstract && x.GetInterfaces().Any(i => i.IsGenericType
                                                    && i.GetGenericTypeDefinition() == typeof (IProcesadorComando<,>)
                                                    && i.GetGenericArguments().First() == comando)); 
        }

        public IProcesadorComando ProcesadorPara(Comando comando)
        {
            return (IProcesadorComando) kernel.Get(procesadores[comando.GetType()]);
        }

        public IProcesadorDispositivo ProcesadorParaDispositivo(Dispositivo dispositivo, Action<IProcesadorDispositivo> callBackFinProcesamiento)
        {
            IProcesadorDispositivo procesador;
            if (dispositivo.EsConcentrador)
            {
                log.Debug("Obteniendo procesador para dispositivo concentrador {0}.", dispositivo.Codigo);
                procesador = kernel.Get<ProcesadorDispositivoConcentrador>(
                    new ConstructorArgument("dispositivo", dispositivo, true),
                    new ConstructorArgument("callBackFinProcesamiento", callBackFinProcesamiento, true));
            } 
            else if (dispositivo.Concentrador == null)
            {
                
                log.Debug("Obtebiendo procesador para dispositivo fisico {0}.", dispositivo.Codigo);
                procesador = kernel.Get<ProcesadorDispositivoFisico>(
                    new ConstructorArgument("dispositivo", dispositivo, true),
                    new ConstructorArgument("callBackFinProcesamiento", callBackFinProcesamiento, true));
            }
            else
            {
                log.Debug("Obteniendo procesador para dispositivo lógico {0}. Concentrador {1}.", dispositivo.Codigo, dispositivo.Concentrador.Codigo);
                procesador = kernel.Get<ProcesadorDispositivoConcentrador>(
                    new ConstructorArgument("dispositivo", dispositivo.Concentrador, true),
                    new ConstructorArgument("callBackFinProcesamiento", callBackFinProcesamiento, true));
            }
            return procesador;
        }
    }
}
