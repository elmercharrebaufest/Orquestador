using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverSensorIntercomunicador : DriverBase, IDriverSensor, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigSensor configSensor;
        private IDriverItc driverItc;
        private string entrada;

        private readonly List<string> eventosSoportados = new List<string> {
            CodigosEventos.EntradaActivada
            ,CodigosEventos.CambioEstadoIntercomunicador
            ,CodigosEventos.ErrorConexionDispositivo
            ,CodigosEventos.ConexionDispositivoCorrecta
        };

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
            Log.Info("DriverSensorIntercomunicador Inicializar");
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
            Log.Info("DriverSensorIntercomunicador OnEventoDriverFisico");

            var notificacion = evento.Notificacion;

            Log.Info("DriverSensorIntercomunicador Codigo:" + codigoDispositivo);
            Log.Info("DriverSensorIntercomunicador Datos: " + JsonConvert.SerializeObject(notificacion, Formatting.Indented));
            var nuevoEvento = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = CodigosEventos.CambioEstadoIntercomunicador,
                    Datos = notificacion.Datos,
                }
            };
            OnEventoDriver(nuevoEvento);
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }
    }
}