using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;
using System;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public abstract class ProcesadorComando<TComando, TResultado> : IProcesadorComando<TComando, TResultado> where TComando : Comando where TResultado : ResultadoComando, new()
    {
        protected ILogger Log { get; private set; }

        protected ProcesadorComando(ILogger log)
        {
            Log = log;
        }

        public ResultadoComando Procesar(Comando comando, Dispositivo dispositivo, IDriver driver)
        {
            return Procesar((TComando)comando, dispositivo, driver);
        }

        public TResultado Procesar(TComando comando, Dispositivo dispositivo, IDriver driver)
        {
            try
            {
                Log.Debug("Procesando Comando: {0}", comando);
                return Ejecutar(comando, dispositivo, driver);
            }
            catch (DriverConMensajeException e)
            {
                return new TResultado
                {
                    Mensaje =
                        new Mensaje(Codigos.ErrorDriver, e.Message,
                                    dispositivo.Codigo)
                };
            }
            catch (ComandoDriverException e)
            {
                Log.Error(e, "Comando sin parametros requeridos {0}", dispositivo.Codigo);
                return new TResultado
                {
                    Mensaje =
                        new Mensaje(Codigos.ComandoInvalido, Textos.ResultadoComando,
                                    dispositivo.Codigo)
                };
            }
            catch (ConexionDispositivoDriverException e)
            {
                Log.Error(e, "Falló la conexión al dispositivo {0}", dispositivo.Codigo);
                return new TResultado
                {
                    Mensaje =
                            new Mensaje(Codigos.ConexionDispositivo, Textos.ResultadoConexionDispositivo + e.Message,
                                        dispositivo.Codigo)
                };
            }
            catch (FormatoRespuestaDriverException e)
            {
                Log.Error(e, "Formato de respuesta del dispositivo {0} incorrecto para el comando {1}",
                          dispositivo.Codigo, comando.GetType());
                return new TResultado
                {
                    Mensaje =
                            new Mensaje(Codigos.FormatoRespuestaDispositivo, Textos.ResultadoFormatoRespuestaDispositivo,
                                        dispositivo.Codigo, comando.GetType().Name)
                };
            }
            catch (LecturaEscrituraDriverException e)
            {
                Log.Error(e, "El dispositivo {0} no puede escribir en la ruta especificada al comando {1}", dispositivo.Codigo, comando.GetType());
                return new TResultado
                {
                    Mensaje = new Mensaje(Codigos.LecturaEscritura, Textos.ResultadoLecturaEscritura, dispositivo.Codigo)
                };
            }
            catch (DriverException e)
            {
                Log.Error(e, "Error al ejecutar el comando {0} en el dispositivo {1}", comando.GetType(), dispositivo.Codigo);
                return new TResultado
                {
                    Mensaje = new Mensaje(Codigos.ErrorDriver, Textos.ResultadoErrorDriver, comando.GetType().Name, dispositivo.Codigo)
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error al ejecutar el comando {0}", comando);
                return new TResultado
                {
                    Mensaje = new Mensaje(Codigos.Error, Textos.ResultadoError)
                };
            }
        }

        protected abstract TResultado Ejecutar(TComando comando, Dispositivo dispositivo, IDriver driver);
    }
}