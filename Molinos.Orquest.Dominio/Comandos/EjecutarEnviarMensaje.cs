namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarEnviarMensaje : ComandoEjecutar
    {
        public string Texto { get; set; }
        public string NumeroPrograma { get; set; }
        public string NumeroTrama { get; set; }
        public string NumeroVariable { get; set; }

        public override string ToString()
        {
            return "Ejecutar EjecutarEnviarMensaje: " + CodigoDispositivo;
        }
    }
}
