using Ninject.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;

namespace Molinos.Orquest.DriversImpl.Helpers
{
    public static class IdentificacionVehicularHelper
    {
        public static string GuardarImagen(ILogger log, byte[] imagen, string contentType, string codigoCiv, string codigoCamara, string estado)
        {
            var rutaFotos = ConfigurationManager.AppSettings["IdentificacionVehicular.RutaFotos"];
            if (string.IsNullOrEmpty(rutaFotos))
                return null;

            try
            {
                var fecha = DateTime.Now.ToString("yyyyMMdd");
                var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var extension = contentType.Substring(contentType.IndexOf("/", StringComparison.Ordinal) + 1);

                var fullPath = string.IsNullOrEmpty(codigoCamara)
                    ? Path.Combine(rutaFotos, fecha, codigoCiv, estado)
                    : Path.Combine(rutaFotos, fecha, codigoCiv, codigoCamara, estado);

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                var filePath = Path.Combine(fullPath, $"{fileName}.{extension}");
                File.WriteAllBytes(filePath, imagen);
                log.Debug("[IdentificacionVehicularImagenHelper] Imagen guardada en {0}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                log.Error(ex, "[IdentificacionVehicularImagenHelper] Error al guardar imagen (civ={0} camara={1}).", codigoCiv, codigoCamara);
                return null;
            }
        }
    }
}
