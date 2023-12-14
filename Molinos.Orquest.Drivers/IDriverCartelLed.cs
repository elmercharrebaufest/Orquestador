namespace Molinos.Orquest.Drivers
{
    public interface IDriverCartelLed : IDriver
    {
        void EnviarMensaje(string texto, string numeroPrograma, string numeroTrama, string numeroVariable);

        void EnviarMensajeIntervalo(string textoPrimario, string textoSecundario, string numeroPrograma, string numeroTrama, string numeroVariable, int intervalMilliseconds);

        void DetenerIntervalo(string numeroPrograma, string numeroTrama, string numeroVariable);
    }
}