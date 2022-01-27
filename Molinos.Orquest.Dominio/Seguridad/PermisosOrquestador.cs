using System.ComponentModel.DataAnnotations;
using Molinos.Orquest.Dominio.Recursos;

namespace Molinos.Orquest.Dominio.Seguridad
{
    public enum PermisosOrquestador
    {
        [Display(ResourceType = typeof(Textos), Name = "Barrera_Administrar")]
        Barrera = 1,
        [Display(ResourceType = typeof(Textos), Name = "Administracion_Cabezales")]
        Cabezal = 2,
        [Display(ResourceType = typeof(Textos), Name = "Camara_Administrar")]
        Camara = 3,
        [Display(ResourceType = typeof(Textos), Name = "Humedimetro_Administrar")]
        Humedimetro = 4,
        [Display(ResourceType = typeof(Textos), Name = "Itc_Administrar")]
        Itc = 5,
        [Display(ResourceType = typeof(Textos), Name = "LectorTarjetas_Administrar")]
        LectorTarjetas = 6,
        [Display(ResourceType = typeof(Textos), Name = "PruebaItc_Diagnostico")]
        PruebaItc = 7,
        [Display(ResourceType = typeof(Textos), Name = "Administracion_Roles")]
        Rol = 8,
        [Display(ResourceType = typeof(Textos), Name = "Sensor_Administrar")]
        Sensor = 9,
        [Display(ResourceType = typeof(Textos), Name = "Suscripciones_Administrar")]
        Suscripcion = 10,
        [Display(ResourceType = typeof(Textos), Name = "Usuario_Administrar")]
        Usuario = 11,
        [Display(ResourceType = typeof(Textos), Name = "ServicioOrquestador")]
        ServicioOrquestador = 12,
        [Display(ResourceType = typeof(Textos), Name = "PruebaCabezal_Diagnostico")]
        PruebaCabezal = 13,
        [Display(ResourceType = typeof(Textos), Name = "PruebaHumedimetro_Diagnostico")]
        PruebaHumedimetro = 14,
        [Display(ResourceType = typeof(Textos), Name = "PruebaCamara_Diagnostico")]
        PruebaCamara = 15,
        [Display(ResourceType = typeof(Textos), Name = "MonitoreoOrquestador")]
        MonitoreoOrquestador = 16,
        [Display(ResourceType = typeof(Textos), Name = "Empresa_Administrar")]
        AbmFirma = 17,
        [Display(ResourceType = typeof(Textos), Name = "BalanzaPuerto_Administrar")]
        BalanzaPuerto = 18,
        [Display(ResourceType = typeof(Textos), Name = "Meteorologica_Administrar")]
        AbmMeteorologica = 19,
        [Display(ResourceType = typeof(Textos), Name = "Nirs_Administrar")]
        Nirs = 20,
        [Display(ResourceType = typeof(Textos), Name = "Nirs_Diagnostico")]
        PruebaNirs = 21,
        [Display(ResourceType = typeof(Textos), Name = "Pantalla_Administrar")]
        Pantalla = 22,
        [Display(ResourceType = typeof(Textos), Name = "Cartel_Led_Administrar")]
        CartelLed = 23,
        [Display(ResourceType = typeof(Textos), Name = "PuestoDeVianda_Administrar")]
        PuestoDeVianda = 24,
        [Display(ResourceType = typeof(Textos), Name = "PruebaPuestoDeVianda_Diagnostico")]
        PruebaPuestoDeVianda = 25,
        [Display(ResourceType = typeof(Textos), Name = "PantallaPuestoDeVianda_Administrar")]
        PantallaPuestoDeVianda = 26,
        [Display(ResourceType = typeof(Textos), Name = "PruebaPantallaPuestoDeVianda_Diagnostico")]
        PruebaPantallaPuestoDeVianda = 27, 
        [Display(ResourceType = typeof(Textos), Name = "ImpresoraHasar_Administrar")]
        ImpresoraHasar = 28,
        [Display(ResourceType = typeof(Textos), Name = "PruebaImpresoraHasar_Diagnostico")]
        PruebaImpresoraHasar = 29,
        [Display(ResourceType = typeof(Textos), Name = "LectorQr_Administrar")]
        LectorQr = 30,
        [Display(ResourceType = typeof(Textos), Name = "PruebaLectorQr_Diagnostico")]
        PruebaLectorQr = 31,
        [Display(ResourceType = typeof(Textos), Name = "Molinete_Administrar")]
        Molinete = 32,
        PruebaMolinete = 33,
        [Display(ResourceType = typeof(Textos), Name = "Display_Administrar")]
        Display = 34,
        [Display(ResourceType = typeof(Textos), Name = "CortinaAgua_Administrar")]
        CortinaAgua = 35,
        [Display(ResourceType = typeof(Textos), Name = "Historian_Administrar")]
        Historian = 36,
        [Display(ResourceType = typeof(Textos), Name = "Tags_Administrar")]
        Tags = 37,
        [Display(ResourceType = typeof(Textos), Name = "Comunicador_Administrar")]
        Comunicador = 38,
        [Display(ResourceType = typeof(Textos), Name = "Intercomunicador_Administrar")]
        Intercomunicador = 39,
    }
}