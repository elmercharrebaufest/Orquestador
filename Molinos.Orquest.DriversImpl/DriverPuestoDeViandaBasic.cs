using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;

namespace Molinos.Orquest.DriversImpl
{
    class DriverPuestoDeViandaBasic : DriverBase, IDriverPuestoDeVianda
    {
        private string codigoDispositivo;
        private ConfigPuestoDeVianda configuracionPuestoVianda;
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.LecturaTarjetaRecibida};

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigPuestoDeVianda); }
        }
        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;
            configuracionPuestoVianda = (ConfigPuestoDeVianda)configuracion;
        }

        public override void VerificarDispositivo()
        {
        }

        public void NotificarLecturaViandas(NotificarLecturaViandas comando)
        {
            var nuevoEvento = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = codigoDispositivo,
                    CodigoEvento = "LecturaTarjetaRecibida",
                    Datos = new Dictionary<string, string>
                    {
                        { "Tarjeta",comando.Tarjeta},
                        { "Sector",configuracionPuestoVianda.Sector}
                    }
                }
            };
            Log.Debug("Notificar suscriptores");
            OnEventoDriver(nuevoEvento);
            Log.Debug("FIN suscriptores");
        }
    }
}
