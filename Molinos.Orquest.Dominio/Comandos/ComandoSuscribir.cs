namespace Molinos.Orquest.Dominio.Comandos
{
    public class ComandoSuscribir : Comando
    {
        public string CodigoEvento { get; set; }
        public string RutaAccesoSuscriptor { get; set; }
        public bool Persistente { get; set; }


        public override string ToString()
        {
            return string.Format("Suscribir {0} Codigo Evento: {1} Suscriptor: {2} Persistente: {3}",
                CodigoDispositivo, CodigoEvento, RutaAccesoSuscriptor, Persistente);
        }
    }
}
