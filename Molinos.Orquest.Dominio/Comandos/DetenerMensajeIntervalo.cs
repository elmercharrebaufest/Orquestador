namespace Molinos.Orquest.Dominio.Comandos
{
    public class DetenerMensajeIntervalo : ComandoEjecutar
    {
        public string NumeroPrograma { get; set; }
        public string NumeroTrama { get; set; }
        public string NumeroVariable { get; set; }

        public override string ToString()
        {
            return "Ejecutar DetenerMensajeIntervalo: " + CodigoDispositivo;
        }
    }
}
