using Microsoft.ApplicationInsights;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios.Procesamiento;
using Ninject.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceProcess;

namespace Molinos.Orquest.Servicios.Impl
{
    public class ServicioOrquestador : IServicioOrquestador
    {
        private readonly IProcesadorFactory factoryProcesador;
        private readonly IRepositorioFactory factoryRepositorio;
        private readonly IServicioRemotoFactory factoryOrqRemoto;
        private readonly INamedLocker locker;
        private readonly IProgramadorTareas programador;
        private readonly ILogger log;
        private readonly TelemetryClient aiClient;

        private readonly IDictionary<string, IProcesadorDispositivo> procesadores;
        private readonly IDictionary<string, bool> estadoDispositivo;
        private bool verificandoDispositivos;
        public string UrlServicio { get; private set; }
        public string NombreMaquina { get; private set; }
        public int IdOrquestador { get; private set; }

        public ServicioOrquestador(IRepositorioFactory factoryRepositorio, IServicioRemotoFactory factoryOrqRemoto, IProcesadorFactory factoryProcesador, INamedLocker locker, IProgramadorTareas programador, ILogger log)
        {
            this.factoryRepositorio = factoryRepositorio;
            this.factoryOrqRemoto = factoryOrqRemoto;
            this.factoryProcesador = factoryProcesador;
            this.locker = locker;
            this.programador = programador;
            this.log = log;
            this.aiClient = new TelemetryClient();

            procesadores = new ConcurrentDictionary<string, IProcesadorDispositivo>();
            estadoDispositivo = new ConcurrentDictionary<string, bool>();
        }

        public void Iniciar(string nombreMaquina, string urlServicio)
        {
            NombreMaquina = nombreMaquina;
            UrlServicio = urlServicio;
            log.Debug("Inicializando orquestador. Servidor: {0} Url: {1}", nombreMaquina, urlServicio);
            RegistrarOrquestador(urlServicio, nombreMaquina);
            log.Debug("Inicialización completa");

            programador.DepurarSuscripciones += (sender, args) => DepurarSuscripriones(args.Vencimiento);
            programador.ActualizarEstado += (sender, args) => VerificarDispositivos();
            programador.Iniciar();
        }

        private void RegistrarOrquestador(string urlServicio, string nombreMaquina)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                var orquestador = repositorio.Obtener<Orquestador>(orq => orq.NombreMaquina == nombreMaquina);
                if (orquestador == null)
                {
                    log.Debug("Registrando orquestador en la base de datos...");
                    orquestador = new Orquestador
                    {
                        NombreMaquina = nombreMaquina,
                        RutaAcceso = urlServicio
                    };
                    repositorio.Agregar(orquestador);
                }
                else
                {
                    log.Debug("El orquestador ya estaba registrado en la base de datos. Actualizando parámetros...");
                    orquestador.RutaAcceso = urlServicio;
                    log.Debug("Liberando dispositivos tomados anteriormente...");
                    repositorio.LiberarDispositivos(orquestador.Id);
                }
                repositorio.GuardarCambios();
                IdOrquestador = orquestador.Id;
            }
        }

        public void VerificarDispositivos()
        {
            if (verificandoDispositivos)
            {
                return;
            }
            verificandoDispositivos = true;
            try
            {
                IList<string> dispositivos = null;
                using (var repositorio = factoryRepositorio.Repositorio())
                {
                    log.Debug("Verificando dispositivos...");
                    ActualizarFechaEstado(repositorio);
                    dispositivos = repositorio.Listar<Dispositivo, string>(disp => disp.Activo && !disp.EsConcentrador, disp => disp.Codigo);
                    repositorio.GuardarCambios();
                }
                var fallosVerificaciones = 0;
                foreach (var codigo in dispositivos)
                {
                    var resultado = Ejecutar(new EjecutarVerificacionDispositivo { CodigoDispositivo = codigo });
                    log.Debug("Dispositivo: {0} Resultado Verificación: ({1}) {2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
                    if (resultado.Mensaje.Codigo != Codigos.OK)
                    {
                        log.Error("Falló la verificación del dispositivo: {0} Resultado Verificación: ({1}) {2}", codigo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
                        fallosVerificaciones++;
                    }
                    ActualizarEstadoDispositivo(codigo, resultado.Mensaje.Codigo == Codigos.OK);
                }
                var totalDispositivos = dispositivos.Count;
                log.Debug("Verificación completa: {0} de {1} dispositivos respondieron correctamente.", totalDispositivos - fallosVerificaciones, totalDispositivos);
            }
            catch (Exception e)
            {
                log.Error(e, "Error al ejecutar tarea programada Verificacion de dispositivos");
            }
            finally
            {
                verificandoDispositivos = false;
            }
        }

        private void ActualizarFechaEstado(IRepositorio repositorio)
        {
            var estado = repositorio.Listar<Estado>().FirstOrDefault();
            if (estado == null)
            {
                estado = new Estado();
                repositorio.Agregar(estado);
            }
            estado.Fecha = DateTime.Now;
        }

        private void ActualizarEstadoDispositivo(string codigo, bool estado)
        {
            if (!estadoDispositivo.ContainsKey(codigo))
            {
                estadoDispositivo.Add(codigo, estado);
                GuardarEstadoDispositivo(codigo, estado);
            }
            if (estadoDispositivo[codigo] != estado)
            {
                estadoDispositivo[codigo] = estado;
                GuardarEstadoDispositivo(codigo, estado);
            }
        }

        private void GuardarEstadoDispositivo(string codigo, bool estado)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                var dispositivo = repositorio.Obtener<Dispositivo>(disp => disp.Codigo == codigo);
                dispositivo.EstadoCorrecto = estado;
                repositorio.GuardarCambios();
            }
        }

        private void DepurarSuscripriones(DateTime fechaVencimiento)
        {
            log.Debug("Depurando suscripciones vencidas anteriores a '{0}'", fechaVencimiento);
            IList<string> dispositivos;
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                dispositivos = repositorio.DispositivosConSuscripcionesVencidas(fechaVencimiento);
            }
            foreach (var dispositivo in dispositivos)
            {
                var comando = new ComandoDepurarSuscripciones
                {
                    CodigoDispositivo = dispositivo,
                    Vencimiento = fechaVencimiento,
                    DepurarPersistentes = false
                };
                var resultado = Procesar<ComandoDepurarSuscripciones, ResultadoComando>(comando, null);
                log.Debug("Dispositivo: {0} Resultado Depuración: ({1}) {2}",
                            comando.CodigoDispositivo, resultado.Mensaje.Codigo, resultado.Mensaje.Descripcion);
            }
        }

        public void Detener()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                log.Debug("Deteniendo orquestador. Servidor: {0} Url: {1}", NombreMaquina, UrlServicio);
                repositorio.LiberarDispositivos(IdOrquestador);
                log.Debug("Liberando dispositivos tomados...");
                repositorio.Remover<Orquestador>(IdOrquestador);
                log.Debug("Des-registrando orquestador...");
                repositorio.GuardarCambios();
                log.Debug("Orquestador detenido");

                IdOrquestador = 0;
                NombreMaquina = null;
                UrlServicio = null;
            }
        }

        public ResultadoEjecutar Ejecutar(ComandoEjecutar comando)
        {
            try
            {
                if (aiClient.IsEnabled())
                {
                    aiClient.TrackTrace($"Ejecutando comando --> {comando.GetType().Name}");
                }
            }
            catch (Exception e)
            {
                log.Error(e, "No se pudo registrar en AI el comando {0}", comando.GetType().Name);
            }

            return Procesar(comando, (servicio, cmd) => servicio.Ejecutar(cmd));
        }

        public ResultadoSuscribir Suscribir(ComandoSuscribir comando)
        {
            log.Debug($"Suscribiendo {comando.CodigoDispositivo}, evento {comando.CodigoEvento}");
            return Procesar(comando, (servicio, cmd) => servicio.Suscribir(cmd));
        }

        public ResultadoCancelarSuscripcion CancelarSuscripcion(ComandoCancelarSuscripcion comando)
        {
            return Procesar(comando, (servicio, cmd) => servicio.CancelarSuscripcion(cmd));
        }

        public ResultadoComando RecargarConfiguracion(string codigoDispositivo)
        {
            return RecargarConfiguracion(codigoDispositivo, false);
        }

        private ResultadoComando RecargarConfiguracion(string codigoDispositivo, bool liberado)
        {
            log.Debug("Recargando configuracion para Dispositivo: {0} - Liberado: {1}", codigoDispositivo, liberado);
            var resultadoComando = new ResultadoComando { Mensaje = Mensaje.ResultadoOK() };
            try
            {
                using (var repositorio = factoryRepositorio.Repositorio())
                {
                    var dispositivo = repositorio.Obtener<Dispositivo>(d => d.Codigo == codigoDispositivo);
                    var orquestador = dispositivo.TomadoPor;
                    if (orquestador == null && dispositivo.Concentrador != null)
                    {
                        // Esto se da en los casos en que se agregó un nuevo dispositivo lógico
                        orquestador = dispositivo.Concentrador.TomadoPor;
                    }
                    if ((orquestador != null && orquestador.Id == IdOrquestador) || (orquestador == null && liberado))
                    {
                        // Tomado por este orquestador
                        var procesador = ObtenerProcesadorParaDispositivo(codigoDispositivo, repositorio, true, true);
                        if (procesador != null)
                        {
                            ProcesamientoFinalizadoParaDispositivo(procesador, true);
                            if ((dispositivo.Concentrador != null && dispositivo.Concentrador.Activo) ||
                                    (dispositivo.Concentrador == null && dispositivo.Activo))
                            {
                                //Recargamos el procesador...
                                procesador = ObtenerProcesadorParaDispositivo(codigoDispositivo, repositorio, true, true);
                                if (procesador != null)
                                {
                                    ProcesamientoFinalizadoParaDispositivo(procesador);
                                }
                            }
                        }
                    }
                    else if (orquestador != null)
                    {
                        log.Debug("El dispositivo {0} esta tomado por otro orquestador. Reenviando operación...");
                        try
                        {
                            log.Debug("Reenviando la operación de recarga de configuracion a {0}", orquestador.NombreMaquina);
                            using (var servicioRemoto = factoryOrqRemoto.CrearServicioOrquestador(orquestador.RutaAcceso))
                            {
                                resultadoComando = servicioRemoto.Servicio.RecargarConfiguracion(codigoDispositivo);
                            }

                            log.Debug("Comando ejecutado. Resultado: {0}", resultadoComando);
                        }
                        catch (WebException e)
                        {
                            log.Error(e, "No se pudo contactar al orquestador {0}. Se liberaran los dispositivos que tiene tomados.", orquestador);
                            repositorio.LiberarDispositivos(orquestador.Id);

                            log.Debug("Todos los dispositivos del orquestador remoto {0} fueron liberados", orquestador.NombreMaquina);
                            log.Debug("Reintentando ejecutar el comando {0}");
                            resultadoComando = RecargarConfiguracion(codigoDispositivo, true);
                        }
                    }
                }

                log.Debug("Recarga de configuración de {0} ejecutada. Código de respuesta: {1}", codigoDispositivo, resultadoComando.Mensaje.Codigo);
            }
            catch (DispositivoNoEncontradoException e)
            {
                log.Error(e, "No se encontró el dispositivo {0}", codigoDispositivo);
                resultadoComando = new ResultadoComando
                {
                    Mensaje =
                        new Mensaje(Codigos.DispositivoInexistente, Textos.ResultadoDispositivoInexistente, codigoDispositivo)
                };
            }
            catch (DriverNoEncontradoException e)
            {
                log.Error(e, "No se encontro el driver del dispoisitivo {0}", codigoDispositivo);
                return new ResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.DriverNoEncontrado, Textos.ResultadoDriverNoEncontrado, e.ClaseDriver, codigoDispositivo)
                };
            }
            catch (TipoDispositivoIncorrectoException e)
            {
                log.Error(e, "El tipo de dispositivo '{0}' no concuerda el comando que se quiere ejecutar {1}", e.TipoDispositivo, codigoDispositivo);
                return new ResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.TipoDispositivoIncorrecto, Textos.ResultadoTipoDispositivoIncorrecto, e.TipoDispositivo, codigoDispositivo)
                };
            }
            catch (TipoDriverIncorrectoException e)
            {
                log.Error(e, "Ocurrió un error al ejecutar la recarga del dispositivo {0}", codigoDispositivo);
                return new ResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.TipoDriverIncorrecto, Textos.ResultadoTipoDriverIncorrecto, e.ClaseDriver, codigoDispositivo)
                };
            }
            catch (Exception e)
            {
                log.Error(e, "Ocurrió un error al ejecutarla recarga del dispositivo {0}", codigoDispositivo);
                resultadoComando = new ResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.Error, Textos.ResultadoError)
                };
            }
            return resultadoComando;
        }

        public IList<DispositivoDto> ListarLectores()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigLectorTarjetas, DispositivoDto>(lectorTarjeta => lectorTarjeta.Dispositivo.Activo && !lectorTarjeta.Dispositivo.EsConcentrador,
                    lectorTarjeta => new DispositivoDto { Codigo = lectorTarjeta.Dispositivo.Codigo, Descripcion = lectorTarjeta.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarBalanzas()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigCabezal, DispositivoDto>(cabezal => cabezal.Dispositivo.Activo && !cabezal.Dispositivo.EsConcentrador,
                    cabezal => new DispositivoDto { Codigo = cabezal.Dispositivo.Codigo, Descripcion = cabezal.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarBalanzasDePuerto()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigBalanzaPuerto, DispositivoDto>(cabezal => cabezal.Dispositivo.Activo && !cabezal.Dispositivo.EsConcentrador,
                    cabezal => new DispositivoDto { Codigo = cabezal.Dispositivo.Codigo, Descripcion = cabezal.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarBarrerasSemaforos()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigBarrera, DispositivoDto>(barrera => barrera.Dispositivo.Activo && !barrera.Dispositivo.EsConcentrador,
                    barrera => new DispositivoDto { Codigo = barrera.Dispositivo.Codigo, Descripcion = barrera.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarCamaras()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigCamara, DispositivoDto>(camara => camara.Dispositivo.Activo && !camara.Dispositivo.EsConcentrador,
                    barrera => new DispositivoDto { Codigo = barrera.Dispositivo.Codigo, Descripcion = barrera.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarEstacionMeteorologica()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigMeteorologica, DispositivoDto>(meteorologica => meteorologica.Dispositivo.Activo && !meteorologica.Dispositivo.EsConcentrador,
                    meteorologica => new DispositivoDto { Codigo = meteorologica.Dispositivo.Codigo, Descripcion = meteorologica.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarHumedimetros()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigHumedimetro, DispositivoDto>(humedimetro => humedimetro.Dispositivo.Activo && !humedimetro.Dispositivo.EsConcentrador,
                    humedimetro => new DispositivoDto { Codigo = humedimetro.Dispositivo.Codigo, Descripcion = humedimetro.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarSensores()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigSensor, DispositivoDto>(sensor => sensor.Dispositivo.Activo && !sensor.Dispositivo.EsConcentrador,
                    sensor => new DispositivoDto { Codigo = sensor.Dispositivo.Codigo, Descripcion = sensor.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarDisplays()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigDisplay, DispositivoDto>(display => display.Dispositivo.Activo && !display.Dispositivo.EsConcentrador,
                    display => new DispositivoDto { Codigo = display.Dispositivo.Codigo, Descripcion = display.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarConcentradores()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigItc, DispositivoDto>(concentrador => concentrador.Dispositivo.Activo && concentrador.Dispositivo.EsConcentrador,
                    concentrador => new DispositivoDto { Codigo = concentrador.Dispositivo.Codigo, Descripcion = concentrador.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarSensoresPorConcentrador(string concentrador)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigSensor, DispositivoDto>(x => x.Dispositivo.Activo && x.Dispositivo.Concentrador != null && x.Dispositivo.Concentrador.Codigo == concentrador,
                    x => new DispositivoDto { Codigo = x.Dispositivo.Codigo });
            }
        }

        /// <summary>
        /// Se encarga del ruteo de cualquier operacion: Ejecutar, Suscribir, CancelarSuscripcion.
        /// Recibe como parametro la operación a realizar para el caso que debe reenviar la operacion a otro orquestador.
        /// </summary>
        /// <typeparam name="TComando">Subclase de comando que utiliza la operacion</typeparam>
        /// <typeparam name="TResultadoComando">subclase de resultado de comando que devuelve la operacion</typeparam>
        /// <param name="comando">comando a procesar</param>
        /// <param name="operacionReenvioComando">Operacion a procesar remotamente. Si es null el comando no se reenvía</param>
        /// <returns></returns>
        private TResultadoComando Procesar<TComando, TResultadoComando>(TComando comando, Func<IServicioOrquestador, TComando, TResultadoComando> operacionReenvioComando) where TComando : Comando where TResultadoComando : ResultadoComando, new()
        {
            TResultadoComando resultadoComando;
            try
            {
                log.Debug("Ejecutando comando: {0}", comando);
                IProcesadorDispositivo procesador;
                Orquestador orquestadorRemoto = null;
                using (var repositorio = factoryRepositorio.Repositorio())
                {
                    do
                    {
                        procesador = ObtenerProcesadorParaDispositivo(comando.CodigoDispositivo, repositorio, true);
                        if (procesador == null)
                        {
                            // Sino se pudo tomar el dispositivo, hay que ver quien lo tiene tomado para reenviar el comando.
                            // Esto esta en un ciclo porque se puede dar que el dispositivo se libere entre que se quiso tomar
                            // y se verifica quien lo tiene.
                            orquestadorRemoto = repositorio.OrquestadorQueTieneTomado(repositorio.ObtenerCodigoDispositivoConcentrador(comando.CodigoDispositivo));
                        }
                    } while (procesador == null && orquestadorRemoto == null);
                }
                if (procesador != null)
                {
                    log.Debug("Encolando comando en la cola de procesamiento para dispositivo {0}", procesador.CodigoDispositivo);
                    resultadoComando = (TResultadoComando)procesador.Procesar(comando);
                }
                else
                {
                    // Reenviar a quien lo tenga tomado...
                    resultadoComando = ReenviarComando(comando, operacionReenvioComando, orquestadorRemoto);
                }
                log.Debug("Comando {0} ejecutado. Código de respuesta: {1}", comando, resultadoComando.Mensaje.Codigo);
            }
            catch (DispositivoNoEncontradoException e)
            {
                log.Error(e, "No se encontró el dispositivo para el comando {0}", comando);
                resultadoComando = new TResultadoComando
                {
                    Mensaje =
                            new Mensaje(Codigos.DispositivoInexistente, Textos.ResultadoDispositivoInexistente,
                                        comando.CodigoDispositivo)
                };
            }
            catch (DriverNoEncontradoException e)
            {
                log.Error(e, "No se encontro el driver del dispoisitivo {0}", comando.CodigoDispositivo);
                return new TResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.DriverNoEncontrado, Textos.ResultadoDriverNoEncontrado, e.ClaseDriver, comando.CodigoDispositivo)
                };
            }
            catch (TipoDispositivoIncorrectoException e)
            {
                log.Error(e, "El tipo de dispositivo '{0}' no concuerda el comando que se quiere ejecutar {1}", e.TipoDispositivo, comando);
                return new TResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.TipoDispositivoIncorrecto, Textos.ResultadoTipoDispositivoIncorrecto, e.TipoDispositivo, comando)
                };
            }
            catch (TipoDriverIncorrectoException e)
            {
                log.Error(e, "Ocurrió un error al ejecutar el comando {0}", comando);
                return new TResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.TipoDriverIncorrecto, Textos.ResultadoTipoDriverIncorrecto, e.ClaseDriver, comando.CodigoDispositivo)
                };
            }
            catch (Exception e)
            {
                log.Error(e, "Ocurrió un error al ejecutar el comando {0}", comando);
                resultadoComando = new TResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.Error, Textos.ResultadoError)
                };
            }
            return resultadoComando;
        }

        private TResultadoComando ReenviarComando<TComando, TResultadoComando>(TComando comando, Func<IServicioOrquestador, TComando, TResultadoComando> operacionReenvioComando,
                                                                               Orquestador orquestadorRemoto)
            where TComando : Comando where TResultadoComando : ResultadoComando, new()
        {
            TResultadoComando resultadoComando;
            log.Debug("El dispositivo {0} ya esta tomado por el orquestador {1}",
                      comando.CodigoDispositivo,
                      orquestadorRemoto.NombreMaquina);

            if (operacionReenvioComando != null)
            {
                try
                {
                    log.Debug("Reenviando el comando {0} a {1}", comando, orquestadorRemoto.NombreMaquina);
                    using (var servicioRemoto = factoryOrqRemoto.CrearServicioOrquestador(orquestadorRemoto.RutaAcceso))
                    {
                        resultadoComando = operacionReenvioComando(servicioRemoto.Servicio, comando);
                    }
                    log.Debug("Comando ejecutado. Resultado: {0}", resultadoComando);
                }
                catch (WebException e)
                {
                    log.Error(e, "No se pudo contactar al orquestador {0}. Se liberaran los dispositivos que tiene tomados.",
                              orquestadorRemoto);
                    using (var repositorio = factoryRepositorio.Repositorio())
                    {
                        repositorio.LiberarDispositivos(orquestadorRemoto.Id);
                    }

                    log.Debug("Todos los dispositivos del orquestador remoto {0} fueron liberados",
                              orquestadorRemoto.NombreMaquina);
                    log.Debug("Reintentando ejecutar el comando {0}");
                    resultadoComando = Procesar(comando, operacionReenvioComando);
                }
            }
            else
            {
                log.Debug("No se indico accion para reenviar el comando {0} a {1}", comando, orquestadorRemoto.NombreMaquina);
                resultadoComando = new TResultadoComando
                {
                    Mensaje = new Mensaje(Codigos.DispositivoTomado,
                                              Textos.ResultadoDispositovoTomado,
                                              comando.CodigoDispositivo, comando)
                };
            }
            return resultadoComando;
        }

        private IProcesadorDispositivo ObtenerProcesadorParaDispositivo(string codigo, IRepositorio repositorio, bool permitirConcentrador = false, bool permitirInactivo = false)
        {
            log.Debug("Obteniendo cola de procesamiento para dispositivo {0}", codigo);

            var dispositivo = repositorio.Obtener<Dispositivo>(disp => disp.Codigo == codigo && (disp.Activo || permitirInactivo) && (!disp.EsConcentrador || permitirConcentrador));
            if (dispositivo == null)
            {
                throw new DispositivoNoEncontradoException(string.Format("El dispositivo {0} no existe o esta desactivado.", codigo));
            }
            var codigoDispositivo = codigo;
            var serverFijo = dispositivo.ServerFijo;

            if (dispositivo.Concentrador != null)
            {
                if (!dispositivo.Concentrador.Activo && !permitirInactivo)
                {
                    throw new DispositivoNoEncontradoException(string.Format("El dispositivo concentrador {0} no existe o esta desactivado.", dispositivo.Concentrador.Codigo));
                }
                codigoDispositivo = dispositivo.Concentrador.Codigo;
                serverFijo = dispositivo.Concentrador.ServerFijo;
            }
            lock (locker.GetLock(codigoDispositivo))
            {
                //cuando tiene fijado un server y no es este no se devuelve procesador
                if (!string.IsNullOrEmpty(serverFijo) && !string.Equals(serverFijo, NombreMaquina, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                IProcesadorDispositivo procesador;
                if (!procesadores.TryGetValue(codigoDispositivo, out procesador))
                {
                    log.Debug("No existe una cola de procesamiento para {0}. Se creará una.", codigoDispositivo);
                    log.Debug("Intentando tomar dispositivo {0}", codigoDispositivo);

                    var tomado = repositorio.TomarDispositivo(IdOrquestador, codigoDispositivo);
                    if (tomado)
                    {
                        try
                        {
                            log.Debug("Dispositivo {0} tomado exitosamente", codigoDispositivo);
                            procesador = factoryProcesador.ProcesadorParaDispositivo(dispositivo, ProcesamientoFinalizadoParaDispositivo);
                            procesadores.Add(codigoDispositivo, procesador);
                        }
                        catch (Exception ex)
                        {
                            // Si ocurrió algún error al instanciar el procesador, liberar el dispositivo
                            repositorio.LiberarDispositivo(IdOrquestador, codigoDispositivo);
                            log.Error(ex, "No se pudo instanciar el procesador para el dispositivo {0}", codigoDispositivo);
                            throw;
                        }
                    }
                    log.Debug("Cola de procesamiento creada para {0}", codigoDispositivo);
                }
                return procesador;
            }
        }

        private void ProcesamientoFinalizadoParaDispositivo(IProcesadorDispositivo procesador)
        {
            ProcesamientoFinalizadoParaDispositivo(procesador, false);
        }

        private void ProcesamientoFinalizadoParaDispositivo(IProcesadorDispositivo procesador, bool forzarLiberacion)
        {
            log.Debug("Cola de procesamiento para dispositivo {0} vacía", procesador.CodigoDispositivo);
            lock (locker.GetLock(procesador.CodigoDispositivo))
            {
                log.Debug("Procesando comandos remanentes para dispositivo {0}", procesador.CodigoDispositivo);
                var puedeLiberar = procesador.ProcesarRemanentes();
                if (puedeLiberar || forzarLiberacion)
                {
                    procesadores.Remove(procesador.CodigoDispositivo);
                    log.Debug("Liberando dispositivo {0}", procesador.CodigoDispositivo);
                    procesador.Dispose();
                    using (var repositorio = factoryRepositorio.Repositorio())
                    {
                        repositorio.LiberarDispositivo(IdOrquestador, procesador.CodigoDispositivo);
                    }
                    log.Debug("Dispositivo {0} liberado: Liberacion Forzada?: {1}", procesador.CodigoDispositivo, forzarLiberacion);
                }
                else
                {
                    log.Debug("El dispositivo {0} tiene suscripciones activas. Se mantendrá tomado.", procesador.CodigoDispositivo);
                }
            }
        }

        public void IniciarServiceOrquestador(string server)
        {
            if (server == Environment.MachineName)
            {
                var serviceName = ConfigurationManager.AppSettings["OrquestadorService"];
                var servController = new ServiceController(serviceName);
                if (servController.Status == ServiceControllerStatus.Stopped)
                {
                    servController.Start();
                }
            }

            var urlOrquestador = String.Format(ConfigurationManager.AppSettings["UrlOrquestador"],
                                                   server);
            var urlsOrquestador = urlOrquestador.Split(',').Select(url => new Uri(url)).ToArray();
            using (var servRemoto = factoryOrqRemoto.CrearServicioOrquestador(urlsOrquestador[0].AbsoluteUri))
            {
                servRemoto.Servicio.IniciarServiceOrquestador(server);
            }
        }

        public void DetenerServiceOrquestador(string server)
        {
            log.Debug("Detener " + server);
            if (server == Environment.MachineName)
            {
                var serviceName = ConfigurationManager.AppSettings["OrquestadorService"];
                var servController = new ServiceController(serviceName);
                if (servController.Status != ServiceControllerStatus.Stopped)
                {
                    servController.Stop();
                }
            }

            var urlOrquestador = String.Format(ConfigurationManager.AppSettings["UrlOrquestador"],
                                                   server);
            var urlsOrquestador = urlOrquestador.Split(',').Select(url => new Uri(url)).ToArray();
            log.Debug("url a detener " + urlsOrquestador[0].AbsoluteUri);
            using (var servRemoto = factoryOrqRemoto.CrearServicioOrquestador(urlsOrquestador[0].AbsoluteUri))
            {
                servRemoto.Servicio.DetenerServiceOrquestador(server);
            }
        }

        public string ObtenerEstadoServiceOrquestador(string server)
        {
            if (server == Environment.MachineName)
            {
                var serviceName = ConfigurationManager.AppSettings["OrquestadorService"];
                var servController = new ServiceController(serviceName);
                return servController.Status.ToString();
            }

            var urlOrquestador = String.Format(ConfigurationManager.AppSettings["UrlOrquestador"], server);
            var urlsOrquestador = urlOrquestador.Split(',').Select(url => new Uri(url)).ToArray();

            var servRemoto = factoryOrqRemoto.CrearServicioOrquestador(urlsOrquestador[0].AbsoluteUri);

            return servRemoto.Servicio.ObtenerEstadoServiceOrquestador(server);
        }

        public IList<DispositivoDto> ListarNirs()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigNirs, DispositivoDto>(nirs => nirs.Dispositivo.Activo && !nirs.Dispositivo.EsConcentrador,
                    nirs => new DispositivoDto { Codigo = nirs.Dispositivo.Codigo, Descripcion = nirs.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarPuestoDeViandas()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigPuestoDeVianda, DispositivoDto>(puestoVianda => puestoVianda.Dispositivo.Activo && !puestoVianda.Dispositivo.EsConcentrador,
                    puestoVianda => new DispositivoDto { Codigo = puestoVianda.Dispositivo.Codigo, Descripcion = puestoVianda.Dispositivo.Descripcion, Sector = puestoVianda.Sector });
            }
        }

        public DispositivoDto ObtenerLectorPorPantalla(string codigoPantalla)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Obtener<ConfigPantallaPuestoDeVianda, DispositivoDto>(
                    pantallaPuestoVianda => pantallaPuestoVianda.Dispositivo.Activo &&
                                            !pantallaPuestoVianda.Dispositivo.EsConcentrador &&
                                            pantallaPuestoVianda.Dispositivo.Codigo == codigoPantalla,
                    pantallaPuestoVianda => new DispositivoDto
                    {
                        Codigo = pantallaPuestoVianda.ConfigPuestoDeVianda.Dispositivo.Codigo,
                        Descripcion = pantallaPuestoVianda.ConfigPuestoDeVianda.Dispositivo.Descripcion,
                        Sector = pantallaPuestoVianda.ConfigPuestoDeVianda.Sector
                    });
            }
        }

        public IList<DispositivoDto> ListarImpresorasHasar()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigImpresoraHasar, DispositivoDto>(puestoVianda => puestoVianda.Dispositivo.Activo && !puestoVianda.Dispositivo.EsConcentrador,
                    puestoVianda => new DispositivoDto { Codigo = puestoVianda.Dispositivo.Codigo, Descripcion = puestoVianda.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarLectoresQr()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigLectorQr, DispositivoDto>(lectorQr => lectorQr.Dispositivo.Activo && !lectorQr.Dispositivo.EsConcentrador,
                    lectorQr => new DispositivoDto { Codigo = lectorQr.Dispositivo.Codigo, Descripcion = lectorQr.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarCartelesLed()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigCartelLed, DispositivoDto>(x => x.Dispositivo.Activo && !x.Dispositivo.EsConcentrador,
                    x => new DispositivoDto { Codigo = x.Dispositivo.Codigo, Descripcion = x.Dispositivo.Descripcion });
            }
        }

        public IList<DispositivoDto> ListarMolinetes()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigMolinete, DispositivoDto>(m => m.Dispositivo.Activo && !m.Dispositivo.EsConcentrador,
                    m => new DispositivoDto { Codigo = m.Dispositivo.Codigo, Descripcion = m.Dispositivo.Descripcion });
            }
        }

        public IList<string> ObtenerUrlPorCamara(string[] codigo)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigCamara, string>(m => m.Dispositivo.Activo && codigo.Contains(m.Dispositivo.Codigo),
                    m => m.UrlStreaming);
            }
        }

        public IList<CamaraDto> ObtenerCamaras(string[] codigo)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigCamara, CamaraDto>(m => m.Dispositivo.Activo && codigo.Contains(m.Dispositivo.Codigo),
                    m => new CamaraDto { Codigo = m.Dispositivo.Codigo, Url = m.UrlStreaming });
            }
        }

        public IList<DispositivoDto> ListarIntercomunicadores()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigComunicador, DispositivoDto>(q => q.Dispositivo.Activo && !q.Dispositivo.EsConcentrador,
                    q => new DispositivoDto
                    {
                        Codigo = q.Dispositivo.Codigo,
                        Descripcion = q.Dispositivo.Descripcion
                    });
            }
        }

        public void PrenderApagarDispositivo(string codigoDispositivo, bool activar, string server)
        {
            Ejecutar(new EjecutarComunicador { CodigoDispositivo = codigoDispositivo, Activar = activar, Tipo = Dominio.Enums.TipoComunicador.Mic, ServerComunicador = server });
            Ejecutar(new EjecutarComunicador { CodigoDispositivo = codigoDispositivo, Activar = activar, Tipo = Dominio.Enums.TipoComunicador.Speaker, ServerComunicador = server });
        }

        public IntercomunicadorDispositivoBaseDto ObtenerIntercomunicadorPuertoDeAudio(string codigoDispositivo)
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                var intercomunicadorDto = new IntercomunicadorDispositivoBaseDto();
                var resultado = repositorio.Obtener<ConfigComunicador, IntercomunicadorDispositivoBaseDto>(x =>
                x.Dispositivo.Codigo == codigoDispositivo, x => new IntercomunicadorDispositivoBaseDto
                {
                    Codigo = x.Dispositivo.Codigo
                    ,
                    PuertoDeAudio = x.PuertoDeAudio
                    ,
                    Sensor = x.Sensor.Codigo
                });
                //var resultado = repositorio.Obtener<ConfigComunicador, IntercomunicadorDispositivoBaseDto>(m => m.Dispositivo.Codigo == codigoDispositivo);
                return resultado;
            }
        }

        public IList<DispositivoDto> ListarTags()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigTag, DispositivoDto>(q => q.Dispositivo.Activo && !q.Dispositivo.EsConcentrador,
                    q => new DispositivoDto
                    {
                        Codigo = q.Dispositivo.Codigo,
                        Descripcion = q.Dispositivo.Descripcion
                    });
            }
        }

        public GrupoBarreraDto ObtenerConfiguracionGrupoBarrera(string codigoGrupoBarrera)
        {
            var result = new GrupoBarreraDto();
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                result = repositorio.Obtener<ConfigGrupoBarrera, GrupoBarreraDto>(x => x.Dispositivo.Codigo == codigoGrupoBarrera && x.Dispositivo.Activo, x => new GrupoBarreraDto
                {
                    Id = x.Id,
                    AgrupadorCodigo = x.Dispositivo.Codigo,
                    AgrupadorClaseDriver = x.ClaseDriver,
                    SensorPrimerCruceCodigo = x.SensorPrimerCruce.Dispositivo.Codigo,
                    SensorSegundoCruceCodigo = x.SensorSegundoCruce.Dispositivo.Codigo,
                    SensorArribaCodigo = x.SensorArriba.Dispositivo.Codigo,
                    SensorAbajoCodigo = x.SensorAbajo.Dispositivo.Codigo,
                    BarreraArribaCodigo = x.BarreraArriba.Dispositivo.Codigo,
                    BarreraAbajoCodigo = x.BarreraAbajo.Dispositivo.Codigo,
                });
            }
            return result;
        }

        public IList<GrupoBarreraDto> ObtenerConfiguracionGrupoBarreraPorSegundoCruce(string codigoSegundoCruce)
        {
            var result = new List<GrupoBarreraDto>();
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                result = repositorio.Listar<ConfigGrupoBarrera, GrupoBarreraDto>(x => x.SensorSegundoCruce.Dispositivo.Codigo == codigoSegundoCruce && x.Dispositivo.Activo, x => new GrupoBarreraDto
                {
                    Id = x.Id,
                    AgrupadorCodigo = x.Dispositivo.Codigo,
                    AgrupadorClaseDriver = x.ClaseDriver,
                    SensorPrimerCruceCodigo = x.SensorPrimerCruce.Dispositivo.Codigo,
                    SensorSegundoCruceCodigo = x.SensorSegundoCruce.Dispositivo.Codigo,
                    SensorArribaCodigo = x.SensorArriba.Dispositivo.Codigo,
                    SensorAbajoCodigo = x.SensorAbajo.Dispositivo.Codigo,
                    BarreraArribaCodigo = x.BarreraArriba.Dispositivo.Codigo,
                    BarreraAbajoCodigo = x.BarreraAbajo.Dispositivo.Codigo,
                }).ToList();
            }
            return result;
        }

        public IList<DispositivoDto> ListarGruposBarrera()
        {
            using (var repositorio = factoryRepositorio.Repositorio())
            {
                return repositorio.Listar<ConfigGrupoBarrera, DispositivoDto>(q => q.Dispositivo.Activo && !q.Dispositivo.EsConcentrador,
                    q => new DispositivoDto
                    {
                        Codigo = q.Dispositivo.Codigo,
                        Descripcion = q.Dispositivo.Descripcion
                    });
            }
        }

  
    }
}