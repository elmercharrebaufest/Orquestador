using System;
using System.Collections.Generic;
using System.Globalization;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverJsonFromIotBox : DriverBase, IDriverJsonFromIotBox, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigJsonFromIotBox configDriver;
        private IDriverItc driverItc;
        private string entrada;
        private readonly List<string> eventosSoportados = new List<string> {CodigosEventos.NuevoTransitoOffline,
            CodigosEventos.ErrorConexionDispositivo, CodigosEventos.ConexionDispositivoCorrecta};

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigJsonFromIotBox); }
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc)value;
                driverItc.EventoDriver += OnEventoDriverFisico;
            }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configDriver = (ConfigJsonFromIotBox)configuracion;
            entrada = configDriver.NumeroEntrada.ToString(CultureInfo.InvariantCulture);
        }
        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }
        private void OnEventoDriverFisico(object sender, EventoDriverEventArgs evento)
        {
            var notificacion = evento.Notificacion;
            if (EsEventoParaDispositivo(notificacion))
            {
                var codigoEvento = notificacion.CodigoEvento;
                // Esto se da cuando "0" es "activada" y "1" es desactivada
             

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
        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento)
                && (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada")
                            || notificacion.Datos["Entrada"] == entrada);
        }
    }
}
