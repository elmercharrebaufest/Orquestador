using System;
using System.Collections.Generic;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverLectorTarjetasItc : DriverBase, IDriverLectorTarjetas, IDriverLogico
    {
        private string codigoDispositivo;
        private ConfigLectorTarjetas configLector;
        private IDriverItc driverItc;
        private readonly List<string> eventosSoportados = new List<string> {CodigosEventos.LecturaTarjetaRecibida, CodigosEventos.ErrorConexionDispositivo, CodigosEventos.ConexionDispositivoCorrecta};

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigLectorTarjetas); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configLector = (ConfigLectorTarjetas) configuracion;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public IDriver DriverFisico
        {
            set
            {
                driverItc = (IDriverItc) value;
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
                                CodigoEvento = notificacion.CodigoEvento,
                                Datos = notificacion.Datos
                            }
                    };
                OnEventoDriver(nuevoEvento);
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return eventosSoportados.Contains(notificacion.CodigoEvento) && 
                   (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Lector") 
                    || notificacion.Datos["Lector"] == configLector.Lector);
        }

        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }
    }
}
