namespace Molinos.Orquest.Dominio.Comandos
{
    public class ComandoCancelarSuscripcion : Comando
    {
        public int IdSuscripcion { get; set; }
        public string RutaAccesoSuscriptor { get; set; }
        public bool CancelarTodas { get; set; }

        public override string ToString()
        {
            return string.Format("Cancelar Suscripción {0} ID: {1} Suscriptor: {2} CancelarTodas: {3}",
                CodigoDispositivo, IdSuscripcion, RutaAccesoSuscriptor, CancelarTodas);
        }
    }
}
