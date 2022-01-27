using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCamaraCCTV : DriverCamara, IDriverCamara
    {
        TcpCommandListener servidor;
        private bool notificaEventos = false;
        private bool? falloUltimaConexion;
        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);
        private readonly List<string> eventosSoportados = new List<string> { CodigosEventos.AlertaDeSeguridad };

        public override IEnumerable<string> EventosSoportados
        {
            get
            {
                return eventosSoportados;
            }
        }

        public DriverCamaraCCTV()
        {
        }


        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCamara); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoCamara = codigo;
            configCamara = (ConfigCamara)configuracion;
            servidor = new TcpCommandListener(configCamara.DireccionIp, configCamara.Puerto ?? 0, configCamara.LongFrase ?? 1024, configCamara.TimeoutLectura, Log, true);
            notificaEventos = true;

            Task.Run(() =>
            {
                while (notificaEventos)
                {
                    try
                    {
                        NotificarAlerta(servidor.RecibirMensaje());
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || falloUltimaConexion.Value)
                        {
                            Log.Debug("Conexion reestablecida con la Rasp {0}", codigoCamara);
                            falloUltimaConexion = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al ConsultarEstado del Rasp {0}", codigoCamara);
                        //Cuando no hay estado anterior se lanza el evento
                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de Rasp={0}", codigoCamara);
                            falloUltimaConexion = true;
                        }
                        Thread.Sleep(configCamara.TimeoutLectura);
                    }
                }
                finCiclo.Set();
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                servidor.Desconectar();
                notificaEventos = false;
                finCiclo.WaitOne();
                finCiclo.Dispose();
            }
        }

        public override bool MantenerConectado()
        {
            return true;
        }

        private void NotificarAlerta(string dato)
        {
            try
            {
                Log.Info($"Alerta lanzada por la cámara:{codigoCamara} Evento={dato}");

                var notification = new NotificacionEvento
                {
                    CodigoDispositivo = codigoCamara,
                    CodigoEvento = CodigosEventos.AlertaDeSeguridad,
                    Datos = new Dictionary<string, string>
                                {
                                    {"Entrada", codigoCamara},
                                    {"Dato", dato}
                                }
                };

                OnEventoDriver(new EventoDriverEventArgs { Notificacion = notification });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "No se pudo notificar el evento ", codigoCamara);
            }
        }

    }

}
