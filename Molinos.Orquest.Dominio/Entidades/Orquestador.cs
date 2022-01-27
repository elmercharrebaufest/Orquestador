namespace Molinos.Orquest.Dominio.Entidades
{
    public class Orquestador
    {
        public virtual int Id { get; set; }
        public virtual string NombreMaquina { get; set; }
        public virtual string RutaAcceso { get; set; }
    }
}
