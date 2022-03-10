using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorBarrera : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigSensor configSensor;
        private IDriverItc driverItc;
        private string entrada;

        private readonly List<string> eventosSoportados = new List<string> {CodigosEventos.EntradaActivada
            ,CodigosEventos.EntradaDesactivada
            ,CodigosEventos.ErrorConexionDispositivo
            ,CodigosEventos.ConexionDispositivoCorrecta
            ,CodigosEventos.CambioEstadoSensor
            ,CodigosEventos.CambioEstadoSensorBarrera};

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
            Log.Info("DriverSensorBarrera Inicializar");
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
                var nuevoEvento = new EventoDriverEventArgs
                {
                    Notificacion = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = CodigosEventos.CambioEstadoSensorBarrera,
                        Datos = notificacion.Datos,
                    }
                };
                OnEventoDriver(nuevoEvento);
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }
    }
}
