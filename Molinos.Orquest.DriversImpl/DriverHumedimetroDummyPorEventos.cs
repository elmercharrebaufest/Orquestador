using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverHumedimetroDummyPorEventos : DriverBase, IDriverHumedimetro
    {
        private bool eventosActivos = false;
        private string codigoDispositivo;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public override IEnumerable<string> EventosSoportados
        {
            //No soporta eventos
            get { return new List<string> {CodigosEventos.HumedadRecibida}; }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoDispositivo = codigo;

            eventosActivos = true;
            Task.Run(() =>
            {
                while (eventosActivos)
                {
                    Thread.Sleep(1000);
                    var notification = new NotificacionEvento
                    {
                        CodigoDispositivo = codigoDispositivo,
                        CodigoEvento = CodigosEventos.HumedadRecibida,
                        Valores =
                            new Dictionary<string, decimal>
                                        {
                                            {"AnalisisHumedad", DateTime.Now.Millisecond%100}
                                        }
                    };

                    OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
                }
            });
        }

        public override void VerificarDispositivo()
        {
        }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            Thread.Sleep(100);
            return DateTime.Now.Millisecond % 100;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                eventosActivos = false;
            }
        }

    }
}
