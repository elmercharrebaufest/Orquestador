namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarEnviarMensajeIntervalo : ComandoEjecutar
    {
        public string Texto { get; set; }
        public string NumeroPrograma { get; set; }
        public string NumeroTrama { get; set; }
        public string NumeroVariable { get; set; }
        public string TextSecundario { get; set; }
        public int Intervalo { get; set; }

        public override string ToString()
        {
            return "Ejecutar EjecutarEnviarMensajeIntervalo: " + CodigoDispositivo;
        }
    }
}
