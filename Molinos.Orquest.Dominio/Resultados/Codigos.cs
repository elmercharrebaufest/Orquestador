namespace Molinos.Orquest.Dominio.Resultados
{
    public static class Codigos
    {
        public const int OK = 0;

        // No es un error, sino que el el dispositivo esta tomado y se indico que no se reenvie el comando
        public const int DispositivoTomado = 001;

        //Error de configuracion de dispositivos
        public const int DispositivoInexistente = 100;
        public const int DriverNoEncontrado = 102;
        public const int TipoDispositivoIncorrecto = 103;
        public const int TipoDriverIncorrecto = 104;

        // Errores de drivers
        public const int ErrorDriver = 200;
        public const int ConexionDispositivo = 201;
        public const int FormatoRespuestaDispositivo = 202;
        public const int CabezalPesoNoEstable = 203;
        public const int CabezalNoFuerzaCero = 204;
        public const int EventoNoSoportado = 205;
        public const int LecturaEscritura = 206;
        public const int SinLecturaDeHumedad = 207;
        public const int ComandoInvalido = 208;

        public const int BalanzadaNoEncontrada = 209;
        public const int ErrorALPR = 210;
        // Errores de suscripciones
        public const int SuscripcionInexistente = 301;
        
        //Error desconocido
        public const int Error = 999;
    }
}
