using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorIotBox : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigSensor configSensor;
        private IDriverItc driverItc;
        private string entrada;

        private readonly List<string> eventosSoportados = new List<string> {CodigosEventos.EntradaActivada
            ,CodigosEventos.EntradaDesactivada
            ,CodigosEventos.ErrorConexionDispositivo
            ,CodigosEventos.ConexionDispositivoCorrecta
            ,CodigosEventos.CambioEstadoSensor};

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
            Log.Info("DriverSensorIotBox Inicializar");
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
            var notificacion = evento.Notificacion;

            if (EsEventoParaDispositivo(notificacion))
            {
                var codigoEvento = ConvertirEvento(notificacion);
                // Esto se da cuando "0" es "activada" y "1" es desactivada
                if (!configSensor.EstadoActivado)
                {
                    codigoEvento = InvertirEventoActivacion(codigoEvento);
                    if (notificacion.Datos.ContainsKey("Mensaje"))
                        notificacion.Datos["Mensaje"] = notificacion.Datos["Mensaje"] == "True" ? "False" : "True";
                }
                var nuevoEvento = new EventoDriverEventArgs
                {
                    Notificacion = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = codigoEvento,
                        Datos = notificacion.Datos,
                    }
                };
                OnEventoDriver(nuevoEvento);
            }
        }

        private static string InvertirEventoActivacion(string codigoEvento)
        {
            if (codigoEvento == CodigosEventos.EntradaActivada)
            {
                codigoEvento = CodigosEventos.EntradaDesactivada;
            }
            else if (codigoEvento == CodigosEventos.EntradaDesactivada)
            {
                codigoEvento = CodigosEventos.EntradaActivada;
            }
            return codigoEvento;
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }

        private string ConvertirEvento(NotificacionEvento notificacion)
        {
            if (notificacion.Datos.ContainsKey("Dato"))
            {
                notificacion.CodigoEvento = notificacion.Datos["Dato"] == "true" ? CodigosEventos.EntradaActivada : CodigosEventos.EntradaDesactivada;
            }
            return notificacion.CodigoEvento;
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }

        public ResultadoEstadoSensor ConsultaEstadoActual()
        {
            Log.Info($"ConsultaEstadoActual Dispositivo : {codigoDispositivo}, Numero Entrada : {configSensor.NumeroEntrada}");

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
    }
}
