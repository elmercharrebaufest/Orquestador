using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Molinos.Orquest.Web.ServicioHub
{
    [HubName("notificarLectura")]
    public class NotificaLectura : Hub
    {
        public void NotificarLecturaTarjeta(LecturaTarjeta lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoItc).actualizarLecturaTarjeta(lectura);
            }
        }

        public void NotificarLecturaEntrada(LecturaEntrada lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoItc).actualizarLecturaEntrada(lectura);
            }
        }

        public void NotificarEstadoDispositivo(EstadoDispositivo estado)
        {
            if (Clients != null)
            {
                Clients.Group(estado.CodigoItc).actualizarEstadoDispositivo(estado);
            }
        }

        public void EscucharItc(string codigoItc)
        {
            Groups.Add(Context.ConnectionId, codigoItc);
        }
        //TODO: Deprecar
        public void EscucharMolinete(string codigoMolinete)
        {
            Groups.Add(Context.ConnectionId, codigoMolinete);
        }
        //TODO: Deprecar
        public void NotificarLecturaTarjetaMolinete(LecturaTarjetaMolinete lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoMolinete).actualizarLecturaTarjeta(lectura);
            }
        }
        //TODO: Deprecar
        public void NotificarTransitoMolinete(TransitoMolinete lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoMolinete).actualizarTransitoMolinete(lectura);
            }
        }
        //TODO: Deprecar
        public void NotificarLecturaDni(LecturaQr lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoMolinete).actualizarLecturaDni(lectura);
                Clients.Group(lectura.CodigoItc).actualizarLecturaQr(lectura);

            }
        }
        public void NotificarLecturaCPE(LecturaQr lectura)
        {
            if (Clients != null)
            {
                Clients.Group(lectura.CodigoItc).actualizarLecturaQr(lectura);
            }
        }
    }

}