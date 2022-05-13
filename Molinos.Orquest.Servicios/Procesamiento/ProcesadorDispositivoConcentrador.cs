using System;
using System.Collections.Generic;
using System.Linq;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public sealed class ProcesadorDispositivoConcentrador : ProcesadorDispositivo
    {
        private readonly IDictionary<string, IDriver> driversLogicos = new Dictionary<string, IDriver>();
        private readonly IDictionary<string, Dispositivo> dispositivosLogicos = new Dictionary<string, Dispositivo>(); 
        
        public ProcesadorDispositivoConcentrador(Dispositivo dispositivo, IRepositorioFactory repositorioFactory, IProcesadorFactory procesadorFactory, IDriverFactory driverFactory, IAdministradorSuscripciones adminSuscripciones, Action<IProcesadorDispositivo> callBackFinProcesamiento, ILogger log) 
            : base(dispositivo, procesadorFactory, driverFactory, adminSuscripciones, callBackFinProcesamiento, log)
        {
            using (var repositorio = repositorioFactory.Repositorio())
            {
                var dispositivos = repositorio.Listar<Dispositivo>(d => d.Concentrador.Id == dispositivo.Id && d.Activo);
                foreach (var disp in dispositivos)
                {
                    dispositivosLogicos.Add(disp.Codigo, disp);
                    log.Debug("Instanciando el driver logico para el dispositivo {0}", disp.Codigo);
                    var driverLogico = driverFactory.DriverLogico<IDriver>(disp, Driver);
                    driversLogicos.Add(disp.Codigo, driverLogico);
                    //Nos suscribimos a los eventos de los drivers logicos
                    driverLogico.EventoDriver += (sender, args) => { adminSuscripciones.Notificar(args.Notificacion);
                        log.Debug($"Suscripcion Notificacion Concentrador: Dispositivos: {args.Notificacion.CodigoDispositivo } Evento: {args.Notificacion.CodigoEvento}");
                    };

                }
            }
        }

        protected override ResultadoComando ProcesarComando(Comando comando)
        {
            try
            {
                Dispositivo disp;
                IDriver driver;
                if (Dispositivo.Codigo == comando.CodigoDispositivo)
                {
                    Log.Debug("Obteniendo dispositivo/driver para el Concentrador {0}", comando.CodigoDispositivo);
                    disp = Dispositivo;
                    driver = Driver;
                }
                else
                {
                    Log.Debug("Obteniendo dispositivo/driver para el dispositivo {0}", comando.CodigoDispositivo);
                    Log.Debug("DispositivosLogicos({0}): {1}", dispositivosLogicos.Count, dispositivosLogicos.Count > 0 ? string.Join(", ",dispositivosLogicos.Keys) : string.Empty);
                    disp = dispositivosLogicos[comando.CodigoDispositivo];
                    driver = driversLogicos[comando.CodigoDispositivo];
                }

                Log.Debug("Obteniendo procesador para el comando {0}", comando);
                var procesadorComando = ProcesadorFactory.ProcesadorPara(comando);
                Log.Debug("Procesando comando {0}", comando);
                return procesadorComando.Procesar(comando, disp, driver);
            }
            catch (KeyNotFoundException e)
            {
                throw new ProcesadorException(
                    string.Format("NO se puede procesar un comando para el dispositivo {0} en el procesador de {1}",
                                    comando.CodigoDispositivo, CodigoDispositivo), e);
            }
        }

        protected override bool PuedeLiberarProcesador()
        {   //evito que se liberen los concentradores sin suscriptores.
            return false;
            //return !dispositivosLogicos.Values.Any(disp => AdminSuscripciones.ExistenSuscripcionesPara(disp.Id));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var driversLogico in driversLogicos.Values)
                {
                    var disposable = driversLogico as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }
    }
}
