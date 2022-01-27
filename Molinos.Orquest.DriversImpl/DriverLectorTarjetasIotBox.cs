using System;
using System.Collections.Generic;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverLectorTarjetasIotBox : DriverBase, IDriverLectorTarjetas, IDriverLogico
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
                                CodigoEvento = ConvertirEvento(notificacion.CodigoEvento),
                                Datos = ConvertirDatos(notificacion.Datos)
                            }
                    };
                OnEventoDriver(nuevoEvento);
            }
        }

        private bool EsEventoParaDispositivo(NotificacionEvento notificacion)
        {
            return (eventosSoportados.Contains(notificacion.CodigoEvento) || notificacion.CodigoEvento == CodigosEventos.EntradaActivada) && 
                   (notificacion.Datos == null || !notificacion.Datos.ContainsKey("Entrada") 
                    || notificacion.Datos["Entrada"] == configLector.Lector);
        }

        private Dictionary<string, string> ConvertirDatos(Dictionary<string,string> datos)
        {
            var resultado = new Dictionary<string, string>();
            if (datos.ContainsKey("Dato"))
            {
                resultado.Add("Tarjeta", datos["Dato"]);
            }
            if (datos.ContainsKey("Entrada"))
            {
                resultado.Add("Lector", datos["Entrada"]);
            }
            foreach(var key in datos.Keys)
            {
                if (key != "Dato" && key != "Entrada")
                {
                    resultado.Add(key, datos[key]);
                }
            }
            return resultado;
        }

        private string ConvertirEvento(string evento)
        {
            return evento == CodigosEventos.EntradaActivada ? CodigosEventos.LecturaTarjetaRecibida : evento;
        }
        public override void InformarEstado()
        {
            driverItc.InformarEstado();
        }
    }
}
