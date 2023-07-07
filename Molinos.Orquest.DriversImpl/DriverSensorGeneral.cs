using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorGeneral : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private string entrada;
        private string codigoEventoITC;
        private ConfigSensor configSensor;
        private IDriverItc driverItc;

        private readonly List<string> eventosSoportados = new List<string> {
             CodigosEventos.EntradaActivada
            , CodigosEventos.EntradaDesactivada
            , CodigosEventos.ErrorConexionDispositivo
            , CodigosEventos.ConexionDispositivoCorrecta
            ,CodigosEventos.CambioEstadoSensor };

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigSensor); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configSensor = (ConfigSensor)configuracion;
            entrada = configSensor.NumeroEntrada.ToString(CultureInfo.InvariantCulture);
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {

            Log.Info("DriverSensorGeneral evento {0}", evento.ToJson());

            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {
                var estado = string.Empty;

                if (notificacion.Datos.ContainsKey("Dato"))
                    estado = notificacion.Datos["Dato"];

                if (estado.ToUpper() == "TRUE")
                {
                    codigoEventoITC = CodigosEventos.EntradaActivada;
                    if (configSensor.Accion != null)
                    {
                        notificacion.Datos["Accion"] = configSensor.Accion.Value.ToString();
                        var eventoNotification = new EventoDriverEventArgs
                        {
                            Notificacion = new NotificacionEvento
                            {
                                CodigoDispositivo = codigoDispositivo,
                                CodigoEvento = CodigosEventos.CambioEstadoSensorGeneral,
                                Datos = notificacion.Datos,
                            }
                        };
                        Log.Info("DriverSensorGeneral EventoNotification {0}", eventoNotification.ToJson());
                        OnEventoDriver(eventoNotification);
                    }
                }
                else
                {
                    codigoEventoITC = CodigosEventos.EntradaDesactivada;
                }

                NotificarEventoITC(notificacion.Datos);
            }
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }

        public ResultadoEstadoSensor ConsultaEstadoActual()
        {
            return new ResultadoEstadoSensor
            {
                CodigoDispositivoSensor = codigoDispositivo,
                EstadoActivo = driverItc.ConsultarEstadoActual(configSensor.NumeroEntrada),
                Mensaje = Mensaje.ResultadoOK()
            };
        }

        public void NotificarEstadoActualSensor()
        {
            driverItc.NotificarEstadoActual(configSensor.NumeroEntrada);
        }

        private void NotificarEventoITC(Dictionary<string, string> datos)
        {
            var eventoNotification = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = codigoEventoITC,
                    Datos = datos
                }
            };
            OnEventoDriver(eventoNotification);
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            Log.Debug($"Es Evento Para Dispositivo: {notificacion.CodigoEvento} Datos: {notificacion.Datos}");
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }
    }
}