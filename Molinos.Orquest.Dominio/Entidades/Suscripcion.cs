using System;

namespace Molinos.Orquest.Dominio.Entidades
{
    public class Suscripcion
    {
        public virtual int Id { get; set; }
        public virtual Dispositivo Dispositivo { get; set; }
        public virtual string CodigoEvento { get; set; }
        public virtual string RutaAccesoSuscriptor { get; set; }
        public virtual int Cantidad { get; set; }
        public virtual DateTime UltimaSuscripcion { get; set; }
        public virtual bool Persistente { get; set; }
    }
}
