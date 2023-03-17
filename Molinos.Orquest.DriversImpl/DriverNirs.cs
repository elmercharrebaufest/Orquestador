using FOSS.Nova.CoreServer.RemoteAPI.Client;
using FOSS.Nova.CoreServer.RemoteAPI.Client.Contract;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverNirs : DriverBase, IDriverNirs
    {
        private string codigoNirs;
        private ConfigNirs configNirs;
        private NovaRemoteAPIProxy cliente;
        private IEnumerable<Product> productos;
        private Sample ultimoAnalisis;
        private SemaphoreSlim semaforoAnalisis;

        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.ErrorConexionDispositivo, CodigosEventos.DispositivoOcupado, CodigosEventos.ConexionDispositivoCorrecta };

        public override IEnumerable<string> EventosSoportados
        {
            get { return eventosSoportados; }
        }
        
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigNirs); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoNirs = codigo;
            configNirs = (ConfigNirs)configuracion;
            ultimoAnalisis = null;
            ConectarNirs();
        }


        public override void InformarEstado()
        {
            Log.Debug("Informando estado NIRS {0}", configNirs);
            try
            {
                cliente.RequestInstrumentState();
            }
            catch(Exception e)
            {
                Log.Error(e, "Error al intentar consultar estado con NIRS {0}", configNirs);
                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, e);
            }
        }

        public override void VerificarDispositivo()
        {
            var huboReconeccion = false;
            do
            {
                try
                {
                    cliente.GetProducts();
                    huboReconeccion = false;
                }
                catch (Exception e)
                {
                    if (huboReconeccion)
                    {
                        Log.Warn(e, "Error al intentar consultar estado con NIRS {0}", configNirs);
                        throw new ConexionDispositivoDriverException(string.Format("Error al conectarse al dispositivo {0}", codigoNirs));
                    }
                    Log.Warn(e, "Intentando reconectarnos ante un error al consultar estado con NIRS {0}", configNirs);
                    cliente.Dispose();
                    ConectarNirs();
                    huboReconeccion = true;
                }
            } while (huboReconeccion);
                      
        }

        public Dictionary<string, decimal> ObtenerAnalisis(string codigoDeMaterial, string codigoDeMuestra)
        {
            var material = productos.FirstOrDefault(x => x.ProductCode == codigoDeMaterial);
            if (material == null) throw new DriverConMensajeException(string.Format("Error, el material {0} no existe en el dispositivo {1}", codigoDeMaterial, codigoNirs));
            var huboReconeccion = false;
            do
            {
                try
                {
                    if (!cliente.Standby()) throw new DriverConMensajeException(string.Format("El dispositivo {0} se encuentra ocupado", codigoNirs));
                    cliente.StartMeasurement(material.ID, codigoDeMuestra, string.Empty, material.UserDefinedFields.ToDictionary(x => x.Name, x => x.Value));
                    semaforoAnalisis = new SemaphoreSlim(0, 1);
                    huboReconeccion = false;
                }
                catch (DriverConMensajeException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    if (huboReconeccion)
                    {
                        Log.Warn(e, "Error al intentar consultar estado con NIRS {0}", codigoNirs);
                        throw new ConexionDispositivoDriverException(string.Format("Error al conectarse al dispositivo {0}", codigoNirs));
                    }
                    Log.Warn(e, "Intentando reconectarnos ante un error al consultar estado con NIRS {0}", codigoNirs);
                    cliente.Dispose();
                    ConectarNirs();
                    huboReconeccion = true;
                }
            } while (huboReconeccion);

            semaforoAnalisis.Wait(configNirs.TimeoutLectura);
            semaforoAnalisis = null;
            var analisisARetornar = ultimoAnalisis;
            ultimoAnalisis = null;
            
            //caracteres a borrar
            var caracteres = !string.IsNullOrEmpty(configNirs.CaracteresABorrar) ? configNirs.CaracteresABorrar.ToCharArray() : new char[0];

            //diccionario a limpiar
            var analisis = analisisARetornar.PrimaryPredictedValues.ToDictionary(x => x.Name, x => Convert.ToDecimal(x.Value));

            //diccionario a retornar
            Dictionary<string, decimal> analisisSinCaracteres = new Dictionary<string, decimal>();

            foreach (var key in analisis.Keys)
            {
                var keyNueva = key;
                foreach (char c in caracteres)
                {
                    keyNueva = keyNueva.Replace(c.ToString(), "");
                }
                analisisSinCaracteres.Add(keyNueva, analisis[key]);
            }

            return analisisARetornar != null ? analisisSinCaracteres :
               throw new DriverConMensajeException(string.Format("Error: el dispositivo {0} no retornó el análisis solicitado", codigoNirs));
        }

        public override bool MantenerConectado()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (semaforoAnalisis != null)
                {
                    semaforoAnalisis.Release();
                }
                cliente.Dispose();
            }
        }

        private void ConectarNirs()
        {
            try
            {
                cliente = new NovaRemoteAPIProxy(configNirs.DireccionIp, configNirs.Puerto, OnNewInstrumentState, OnNewSample);
                if (!cliente.Initialize())
                {
                    var mensaje = string.Format("Error al inicializar conexión al dispositivo {0}, error: {1}", codigoNirs, TryAddLastError());
                    Log.Error(mensaje);
                    throw new ConexionDispositivoDriverException(mensaje);
                }
                productos = cliente.GetProducts();
                cliente.RequestInstrumentState();
            }
            catch (Exception e)
            {
                var mensaje = string.Format("Error al inicializar conexión al dispositivo {0}, error: {1}", codigoNirs, TryAddLastError());
                Log.Error(mensaje);
                throw new ConexionDispositivoDriverException(mensaje);
            }
        }
        
        private void NotificarEstadoConexion(string codigoEvento, Exception e = null)
        {
            try
            {
                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoNirs,
                    CodigoEvento = codigoEvento,
                    Datos = e != null ? new Dictionary<string, string>
                            {
                                {"Error", e.Message},
                                {"Detalle", e.StackTrace}
                            } : new Dictionary<string, string>()
                };
                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "NIRS {0}: No se pudo notificar el evento {1}", codigoNirs, codigoEvento);
            }
        }

        private void OnNewSample(string clientName, Sample sample)
        {            
            Log.Info("New sample received (ID: {0}, SampleNumber: {1}, Client: {2}).", sample.ID, sample.SampleNumber, clientName);
            //if (clientName != )
            if (sample.Subsamples.Any())
            {
                ultimoAnalisis = sample;
            }
            if (semaforoAnalisis != null) semaforoAnalisis.Release();
        }

        private void OnNewInstrumentState(InstrumentState state)
        {            
            if (state != null)
            {
                NotificarEstadoConexion(state.Status != InstrumentStatus.Standby ? CodigosEventos.DispositivoOcupado : CodigosEventos.ConexionDispositivoCorrecta);
                Log.Debug("New instrument state received (status: {0}).", state.Status);
                if (state.Status == InstrumentStatus.Standby && semaforoAnalisis != null) 
                {
                    semaforoAnalisis.Release();
                }
            }
            else
            {
                var error = TryAddLastError();
                Log.Error(error);
                NotificarEstadoConexion(CodigosEventos.ErrorConexionDispositivo, new Exception(error));
            }
        }

        private string TryAddLastError()
        {
            var error = string.Empty;
            if (cliente == null)
            {
                error = "Unable to get last error information from Nova " + codigoNirs;                
            }
            else
            {
                try
                {
                    string lastError = cliente.GetLastError();
                    if (!string.IsNullOrEmpty(lastError))
                    {
                        error = string.Format("Last error information from Nova {1}: {0},  ", lastError, codigoNirs);
                    }
                }
                catch (Exception exception)
                {
                    error = string.Format("Failed to get last error information from Nova {1}: {0}", exception.Message, codigoNirs);
                }
            }
            return error;
        }
    }
}
