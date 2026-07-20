namespace Molinos.Orquest.Dominio
{
    public static class Constantes
    {
        public struct IntercomunicadorDireccion
        {
            public const string HaciaLaWeb = "2web";
            public const string DesdeLaWeb = "web2";
        }

        public struct NotificacionGrupos
        {
            public const string Intercomunicador = "Intercomunicador";
        }

        public struct Drivers
        {
            public const string DriverSensorIntercomunicador = "Molinos.Orquest.DriversImpl.DriverSensorIntercomunicador, Molinos.Orquest.DriversImpl";
            public const string DriverSensorVehicular = "Molinos.Orquest.DriversImpl.DriverSensorVehicular, Molinos.Orquest.DriversImpl";
            public const string DriverLectorPatente = "Molinos.Orquest.DriversImpl.DriverLectorPatente, Molinos.Orquest.DriversImpl";
        }

        public struct TiposDrivers
        {
            public const string Sensor = "Sensor";
            public const string Camara = "Camara";
        }

    }
}