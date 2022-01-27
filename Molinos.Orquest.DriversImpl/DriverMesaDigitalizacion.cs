using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverMesaDigitalizacion : DriverCamara
    {
        protected override byte[] Procesar(string filePath, string subPath, string fileName, HttpWebResponse response, Stream inputStream, bool retornarImagen)
        {
            using (Image image = Image.FromStream(inputStream))
            {
                using(Image imagenCortada = CropImage(image, configCamara.MargenIzquierdo, configCamara.MargenDerecho, configCamara.MargenSuperior, configCamara.MargenInferior))
                {
                    if (configCamara.RotateFlipType.HasValue)
                    {
                        imagenCortada.RotateFlip(configCamara.RotateFlipType.Value);

                    }
                    using (var ms = new MemoryStream())
                    {
                        imagenCortada.Save(ms, ImageFormat.Jpeg);

                        if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(subPath) && !string.IsNullOrEmpty(fileName))
                        {
                            ms.Position = 0;
                            GuardarImagen(filePath, subPath, fileName, response, ms);
                        }
                        return ms.ToArray();
                    }
                }                
            }
        }

        private static Image CropImage(Image img, int? margenIzq, int? margenDer, int? margenTop, int? margenInf)
        {
            Bitmap bmpImage = new Bitmap(img);
            
            return bmpImage.Clone(new Rectangle(margenIzq ?? 0, margenTop ?? 0, img.Width - (margenIzq ?? 0) - (margenDer ?? 0), img.Height - (margenTop ?? 0) - (margenInf ?? 0)), bmpImage.PixelFormat);
        }

    }
}
