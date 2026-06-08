using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Molinos.Orquest.Servicios.Impl
{
    public class AdministradorIdentificacionVehicular : IAdministradorIdentificacionVehicular
    {
        private readonly IRepositorioFactory repositorioFactory;
        private readonly IServicioRemotoFactory servicioRemotoFactory;
        private readonly ILogger log;
        private readonly object syncRoot = new object();
        private int idOrquestador;

        private readonly Dictionary<string, DriverIdentificacionVehicular> drivers
            = new Dictionary<string, DriverIdentificacionVehicular>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ConfigIdentificacionVehicular> configsPendientes
            = new Dictionary<string, ConfigIdentificacionVehicular>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, IDriver> activeDrivers
            = new Dictionary<string, IDriver>(StringComparer.OrdinalIgnoreCase);

        public AdministradorIdentificacionVehicular(
            IRepositorioFactory repositorioFactory,
            IServicioRemotoFactory servicioRemotoFactory,
            ILogger log)
        {
            this.repositorioFactory = repositorioFactory;
            this.servicioRemotoFactory = servicioRemotoFactory;
            this.log = log;
        }

        public void Iniciar(int idOrquestador)
        {
            log.Debug("AdministradorIdentificacionVehicular.Iniciar() idOrquestador={0}", idOrquestador);
            this.idOrquestador = idOrquestador;

            var configs = new List<ConfigIdentificacionVehicular>();
            using (var repo = repositorioFactory.Repositorio())
            {
                repo.LiberarCIVs(idOrquestador);
                var candidatos = repo.Listar<ConfigIdentificacionVehicular>(c => c.Activo && c.TomadoPor == null);
                foreach (var c in candidatos)
                {
                    if (repo.TomarCIV(idOrquestador, c.Codigo))
                    {
                        CargarNavegacion(c);
                        configs.Add(c);
                    }
                }
            }

            lock (syncRoot)
            {
                foreach (var config in configs)
                    configsPendientes[config.Codigo] = config;
            }

            log.Debug("AdministradorIdentificacionVehicular iniciado con {0} configs pendientes.", configs.Count);
        }

        public ResultadoSuscribir Suscribir(string codigoCIV, string codigoEvento, string rutaAccesoSuscriptor)
        {
            lock (syncRoot)
            {
                if (!drivers.ContainsKey(codigoCIV) && !configsPendientes.ContainsKey(codigoCIV))
                {
                    log.Warn("Suscribir: CIV {0} no encontrado.", codigoCIV);
                    return new ResultadoSuscribir { Mensaje = new Mensaje(Codigos.DispositivoInexistente, Textos.ResultadoDispositivoInexistente, codigoCIV) };
                }
            }

            using (var repo = repositorioFactory.Repositorio())
            {
                var existente = repo.Obtener<SuscripcionIdentificacionVehicular>(
                    s => s.ConfigIdentificacionVehicular.Codigo == codigoCIV
                      && s.CodigoEvento == codigoEvento
                      && s.RutaAccesoSuscriptor == rutaAccesoSuscriptor);

                if (existente != null)
                {
                    log.Debug("Suscribir: suscripcion existente id={0} para {1}/{2}.", existente.Id, codigoCIV, codigoEvento);
                    return new ResultadoSuscribir { IdSuscripcion = existente.Id, Mensaje = Mensaje.ResultadoOK() };
                }

                var config = repo.Obtener<ConfigIdentificacionVehicular>(c => c.Codigo == codigoCIV);
                var suscripcion = new SuscripcionIdentificacionVehicular
                {
                    ConfigIdentificacionVehicular = config,
                    CodigoEvento = codigoEvento,
                    RutaAccesoSuscriptor = rutaAccesoSuscriptor
                };
                repo.Agregar(suscripcion);
                repo.GuardarCambios();

                log.Debug("Suscribir: suscripcion id={0} creada para {1}/{2} -> {3}.", suscripcion.Id, codigoCIV, codigoEvento, rutaAccesoSuscriptor);
                return new ResultadoSuscribir { IdSuscripcion = suscripcion.Id, Mensaje = Mensaje.ResultadoOK() };
            }
        }

        public ResultadoComando CancelarSuscripcion(string codigoCIV, string codigoEvento, string rutaAccesoSuscriptor)
        {
            using (var repo = repositorioFactory.Repositorio())
            {
                var suscripcion = repo.Obtener<SuscripcionIdentificacionVehicular>(
                    s => s.ConfigIdentificacionVehicular.Codigo == codigoCIV
                      && s.CodigoEvento == codigoEvento
                      && s.RutaAccesoSuscriptor == rutaAccesoSuscriptor);

                if (suscripcion == null)
                {
                    log.Warn("CancelarSuscripcion: suscripcion no encontrada para CIV {0}/{1} -> {2}.", codigoCIV, codigoEvento, rutaAccesoSuscriptor);
                    return new ResultadoComando { Mensaje = new Mensaje(Codigos.Error, "Suscripcion no encontrada.") };
                }

                repo.Remover(suscripcion);
                repo.GuardarCambios();

                log.Debug("CancelarSuscripcion: suscripcion eliminada para {0}/{1} -> {2}.", codigoCIV, codigoEvento, rutaAccesoSuscriptor);
                return new ResultadoComando { Mensaje = Mensaje.ResultadoOK() };
            }
        }

        public IList<EstadoDispositivoCIVDto> ObtenerEstadoDispositivosCIV(string codigoCIV)
        {
            lock (syncRoot)
            {
                if (!drivers.TryGetValue(codigoCIV, out var driver))
                    return new List<EstadoDispositivoCIVDto>();
                return driver.ObtenerEstadoActual().estado;
            }
        }

        public IList<NotificacionCIVDto> ObtenerUltimasNotificacionesCIV(string codigoCIV)
        {
            lock (syncRoot)
            {
                if (!drivers.TryGetValue(codigoCIV, out var driver))
                    return new List<NotificacionCIVDto>();
                return driver.ObtenerEstadoActual().notificaciones;
            }
        }

        public void RegistrarNotificacionExterna(string codigoCIV, NotificacionCIVDto notificacion)
        {
            lock (syncRoot)
            {
                if (drivers.TryGetValue(codigoCIV, out var driver))
                {
                    driver.RegistrarNotificacion(notificacion);
                    log.Debug("[CIV:{0}] Notificacion externa registrada en buffer.", codigoCIV);
                }
                else
                {
                    log.Warn("[CIV:{0}] RegistrarNotificacionExterna: driver no encontrado.", codigoCIV);
                }
            }
        }

        private void NotificarEvento(NotificacionEvento evento)
        {
            log.Debug("[CIV] Evento: {0}", evento);
            IList<string> rutasSuscriptores;
            using (var repo = repositorioFactory.Repositorio())
            {
                rutasSuscriptores = repo.Listar<SuscripcionIdentificacionVehicular, string>(
                    s => s.ConfigIdentificacionVehicular.Codigo == evento.CodigoDispositivo
                      && s.CodigoEvento == evento.CodigoEvento,
                    s => s.RutaAccesoSuscriptor);
            }

            log.Debug("[CIV] Notificar {0}/{1}: {2} suscriptores.", evento.CodigoDispositivo, evento.CodigoEvento, rutasSuscriptores.Count);
            Task.Run(() =>
            {
                foreach (var ruta in rutasSuscriptores)
                {
                    try
                    {
                        using (var suscriptor = servicioRemotoFactory.CrearServicioSuscriptor(ruta))
                            suscriptor.Servicio.Recibir(evento);
                    }
                    catch (Exception ex)
                    {
                        log.Warn(ex, "[CIV] Notificar: error al notificar {0} -> {1}.", evento.CodigoDispositivo, ruta);
                    }
                }
            });
        }

        public void Detener()
        {
            log.Debug("AdministradorIdentificacionVehicular.Detener()");
            lock (syncRoot)
            {
                foreach (var entry in drivers)
                    entry.Value.Dispose();
                drivers.Clear();
                configsPendientes.Clear();
                activeDrivers.Clear();
            }
            using (var repo = repositorioFactory.Repositorio())
                repo.LiberarCIVs(idOrquestador);

            log.Debug("AdministradorIdentificacionVehicular detenido.");
        }

        public void RecargarConfig(string codigoCIV)
        {
            log.Debug("RecargarConfig: {0}", codigoCIV);

            ConfigIdentificacionVehicular config;
            bool tomado = false;
            using (var repo = repositorioFactory.Repositorio())
            {
                config = repo.Obtener<ConfigIdentificacionVehicular>(c => c.Codigo == codigoCIV);
                if (config != null && config.Activo)
                {
                    // Si ya lo tenemos tomado, no hace falta intentar tomarlo de nuevo
                    if (config.TomadoPorId == idOrquestador)
                        tomado = true;
                    else
                        tomado = repo.TomarCIV(idOrquestador, codigoCIV);

                    if (tomado)
                        CargarNavegacion(config);
                }
            }

            bool liberarCIV = false;

            lock (syncRoot)
            {
                var existeDriver  = drivers.TryGetValue(codigoCIV, out var dipExistente);
                var estaPendiente = configsPendientes.ContainsKey(codigoCIV);

                if (config == null)
                {
                    if (existeDriver)  
                    { 
                        DestruirDriver(codigoCIV, dipExistente); 
                        liberarCIV = true; 
                    }

                    if (estaPendiente) 
                    { 
                        configsPendientes.Remove(codigoCIV); 
                        liberarCIV = true; 
                    }

                    log.Warn("RecargarConfig: no se encontro CIV con codigo {0}.", codigoCIV);
                }
                else if (!config.Activo)
                {
                    if (existeDriver)  
                    { 
                        DestruirDriver(codigoCIV, dipExistente); 
                        liberarCIV = true; 
                    }
                    
                    if (estaPendiente) 
                    { 
                        configsPendientes.Remove(codigoCIV); 
                        liberarCIV = true; 
                    }
                }
                else if (!tomado)
                {
                    if (existeDriver)  DestruirDriver(codigoCIV, dipExistente);
                    if (estaPendiente) configsPendientes.Remove(codigoCIV);
                    log.Debug("RecargarConfig: {0} ya tomado por otro orquestador.", codigoCIV);
                }
                else if (existeDriver)
                {
                    ActualizarDriver(config, dipExistente);
                }
                else
                {
                    configsPendientes[codigoCIV] = config;
                    IntentarCrearDriverPendiente(codigoCIV);
                }
            }

            if (liberarCIV)
            {
                using (var repo = repositorioFactory.Repositorio())
                {
                    repo.LiberarCIV(idOrquestador, codigoCIV);
                }

                log.Debug("RecargarConfig: CIV {0} liberado del orquestador {1}.", codigoCIV, idOrquestador);
            }
        }

        public void ConectarDriver(string codigoDispositivo, IDriver driver)
        {
            lock (syncRoot)
            {
                if (!EstaAsociadoCIV(codigoDispositivo))
                    return;

                log.Debug("ConectarDriver: {0}", codigoDispositivo);
                activeDrivers[codigoDispositivo] = driver;

                foreach (var codigoCIV in new List<string>(configsPendientes.Keys))
                    IntentarCrearDriverPendiente(codigoCIV);

                foreach (var entry in drivers)
                {
                    var dip = entry.Value;
                    var cfg = dip.Config;
                    if (cfg == null) continue;

                    if (EsLector(cfg, codigoDispositivo) && driver is IDriverLectorTarjetas lector)
                    {
                        log.Debug("ConectarDriver: lector {0} -> CIV activo {1}", codigoDispositivo, entry.Key);
                        dip.AsignarTriggerDriverLectorTarjeta(lector);
                    }
                    else if (EsSensorVehicular(cfg, codigoDispositivo) && driver is IDriverSensor sensorV)
                    {
                        log.Debug("ConectarDriver: sensor vehicular {0} -> CIV activo {1}", codigoDispositivo, entry.Key);
                        dip.AsignarTriggerDriverSensor(sensorV);
                    }
                    else if (EsSensorPresencia(cfg, codigoDispositivo) && driver is IDriverSensor sensorP)
                    {
                        log.Debug("ConectarDriver: sensor presencia {0} -> CIV activo {1}", codigoDispositivo, entry.Key);
                        dip.AsignarDriverPresencia(sensorP);
                    }

                }
            }
        }

        public void DesconectarDriver(string codigoDispositivo)
        {
            lock (syncRoot)
            {
                if (!activeDrivers.ContainsKey(codigoDispositivo))
                    return;

                log.Debug("DesconectarDriver: {0}", codigoDispositivo);
                activeDrivers.Remove(codigoDispositivo);

                foreach (var entry in drivers)
                {
                    var dip = entry.Value;
                    var cfg = dip.Config;
                    if (cfg == null) continue;

                    if (EsLector(cfg, codigoDispositivo))
                    {
                        log.Debug("DesconectarDriver: lector {0} de CIV {1}", codigoDispositivo, entry.Key);
                        dip.AsignarTriggerDriverLectorTarjeta(null);
                    }
                    else if (EsSensorVehicular(cfg, codigoDispositivo))
                    {
                        log.Debug("DesconectarDriver: sensor vehicular {0} de CIV {1}", codigoDispositivo, entry.Key);
                        dip.AsignarTriggerDriverSensor(null);
                    }
                    else if (EsSensorPresencia(cfg, codigoDispositivo))
                    {
                        log.Debug("DesconectarDriver: sensor presencia {0} de CIV {1}", codigoDispositivo, entry.Key);
                        dip.AsignarDriverPresencia(null);
                    }

                }
            }
        }

        private void IntentarCrearDriverPendiente(string codigoCIV)
        {
            if (!configsPendientes.TryGetValue(codigoCIV, out var config))
                return;

            var codigoLector  = config.ConfigLectorTarjetas?.Dispositivo?.Codigo;
            var codigoSensorV = config.ConfigSensorVehicular?.Dispositivo?.Codigo;

            bool triggerDisponible =
                (codigoLector  != null && activeDrivers.ContainsKey(codigoLector))
             || (codigoLector  == null && codigoSensorV != null && activeDrivers.ContainsKey(codigoSensorV));

            if (!triggerDisponible)
            {
                log.Debug("CIV pendiente {0}: trigger principal no disponible aun.", codigoCIV);
                return;
            }

            configsPendientes.Remove(codigoCIV);
            CrearDriver(config);
        }

        private void CrearDriver(ConfigIdentificacionVehicular config)
        {
            var dip = new DriverIdentificacionVehicular(config, NotificarEvento, log);
            drivers[config.Codigo] = dip;

            var codigoLector = config.ConfigLectorTarjetas?.Dispositivo?.Codigo;
            var codigoSensorV = config.ConfigSensorVehicular?.Dispositivo?.Codigo;
            var codigoPresencia = config.ConfigSensorPresencia?.Dispositivo?.Codigo;

            if (codigoLector != null && activeDrivers.TryGetValue(codigoLector, out var drL) && drL is IDriverLectorTarjetas lector)
            {
                log.Debug("CrearDriver: lector {0} -> {1}", codigoLector, config.Codigo);
                dip.AsignarTriggerDriverLectorTarjeta(lector);
            }

            if (codigoSensorV != null && activeDrivers.TryGetValue(codigoSensorV, out var drSV) && drSV is IDriverSensor sensorV)
            {
                log.Debug("CrearDriver: sensor vehicular {0} -> {1}", codigoSensorV, config.Codigo);
                dip.AsignarTriggerDriverSensor(sensorV);
            }

            if (codigoPresencia != null && activeDrivers.TryGetValue(codigoPresencia, out var drP) && drP is IDriverSensor sensorP)
            {
                log.Debug("CrearDriver: sensor presencia {0} -> {1}", codigoPresencia, config.Codigo);
                dip.AsignarDriverPresencia(sensorP);
            }

            log.Debug("Driver CIV {0} creado.", config.Codigo);
        }

        private void ActualizarDriver(ConfigIdentificacionVehicular config, DriverIdentificacionVehicular dip)
        {
            dip.ActualizarConfig(config);

            var codigoLector = config.ConfigLectorTarjetas?.Dispositivo?.Codigo;
            var codigoSensorV = config.ConfigSensorVehicular?.Dispositivo?.Codigo;
            var codigoPresencia = config.ConfigSensorPresencia?.Dispositivo?.Codigo;

            activeDrivers.TryGetValue(codigoLector ?? string.Empty, out var drL);
            activeDrivers.TryGetValue(codigoSensorV ?? string.Empty, out var drSV);
            activeDrivers.TryGetValue(codigoPresencia ?? string.Empty, out var drP);

            dip.AsignarTriggerDriverLectorTarjeta(codigoLector != null ? drL  as IDriverLectorTarjetas : null);
            dip.AsignarTriggerDriverSensor(codigoSensorV != null ? drSV as IDriverSensor : null);
            dip.AsignarDriverPresencia(codigoPresencia != null ? drP  as IDriverSensor : null);

            log.Debug("Driver CIV {0} actualizado.", config.Codigo);
        }

        private void DestruirDriver(string codigoCIV, DriverIdentificacionVehicular dip)
        {
            dip.Dispose();
            drivers.Remove(codigoCIV);
            log.Debug("Driver CIV {0} destruido.", codigoCIV);
        }

        private bool EstaAsociadoCIV(string codigo)
        {
            foreach (var c in configsPendientes.Values)
                if (EsLector(c, codigo) || EsSensorVehicular(c, codigo) || EsSensorPresencia(c, codigo))
                    return true;

            foreach (var entry in drivers)
            {
                var cfg = entry.Value.Config;
                if (cfg != null && (EsLector(cfg, codigo) || EsSensorVehicular(cfg, codigo) || EsSensorPresencia(cfg, codigo)))
                    return true;
            }

            return false;
        }

        private static bool EsLector(ConfigIdentificacionVehicular c, string codigo) =>
            string.Equals(c.ConfigLectorTarjetas?.Dispositivo?.Codigo, codigo, StringComparison.OrdinalIgnoreCase);

        private static bool EsSensorVehicular(ConfigIdentificacionVehicular c, string codigo) =>
            string.Equals(c.ConfigSensorVehicular?.Dispositivo?.Codigo, codigo, StringComparison.OrdinalIgnoreCase);

        private static bool EsSensorPresencia(ConfigIdentificacionVehicular c, string codigo) =>
            string.Equals(c.ConfigSensorPresencia?.Dispositivo?.Codigo, codigo, StringComparison.OrdinalIgnoreCase);

        private static void CargarNavegacion(ConfigIdentificacionVehicular c)
        {
            var _ = c.ConfigLectorTarjetas?.Dispositivo?.Codigo;
            var __ = c.ConfigSensorPresencia?.Dispositivo?.Codigo;
            var ___ = c.ConfigSensorVehicular?.Dispositivo?.Codigo;
            if (c.Camaras != null)
                foreach (var cam in c.Camaras)
                {
                    var _u = cam.ConfigCamara?.Uri;
                    var _c = cam.ConfigCamara?.Dispositivo?.Codigo;
                }
        }
    }
}