using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.ModuloALPR.ALPR;
using Ninject.Extensions.Logging;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Molinos.Orquest.ModuloALPR.Impl
{
    public class ServicioALPR: IServicioALPR
    {
        private readonly ILogger log;

        public ServicioALPR(ILogger log)
        {
            this.log = log;
        }
        private static readonly object LockObject = new object();
        public ResultadoObtenerPatente LeerPatente(byte[] imagen, int margenIzquierdo, int margenDerecho, int margenSuperior, int margenInferior)
        {
            var resultado = new ResultadoObtenerPatente();

            var region = "ar";
            String config_file = Path.Combine(AssemblyDirectory + "\\..\\ALPR\\dll", "openalpr.conf");
            String runtime_data_dir = Path.Combine(AssemblyDirectory + "\\..\\ALPR\\dll", "runtime_data");
            var licencia = ConfigurationManager.AppSettings["LicenciaALPR"];
            var rutaImagenes = ConfigurationManager.AppSettings["RutaImagenes"];
            var guardarImagenes = ConfigurationManager.AppSettings["GuardarImagenes"];
            var guidRequest = Guid.NewGuid().ToString();

            lock (LockObject)
            {
                using (var alpr = new Alpr(region, log, config_file, runtime_data_dir, licencia))
                {
                    bool success = alpr.Initialize();

                    if (!success || !alpr.IsLoaded())
                    {
                        log.Error("Error inicializando ALPR");
                        resultado.Mensaje = new Mensaje(Codigos.ErrorALPR, "Error inicializando ALPR");
                        return resultado;
                    }

                    alpr.setTopN(1);
                    for (int intento = 1; intento <= 6; intento++)
                    {
                            using (MemoryStream mStream = new MemoryStream(imagen))
                            {
                                using (Image img = Image.FromStream(mStream))
                                {
                                    // Modificar los márgenes según el intento
                                    int margenIzqModificado = margenIzquierdo;
                                    int margenDerModificado = margenDerecho;
                                    int margenTopModificado = margenSuperior;
                                    int margenInfModificado = margenInferior;
                                    Image imgProcesada = img;

                                    switch (intento)
                                    {
                                        case 2: // Reducir márgenes en un 5% de la imagen que viene de parametro
                                            margenIzqModificado = (int)(0.05 * img.Width);
                                            margenDerModificado = (int)(0.05 * img.Width);
                                            margenTopModificado = (int)(0.05 * img.Height);
                                            margenInfModificado = (int)(0.05 * img.Height);
                                        break;

                                        case 3: // Rotar imagen 20 grados
                                            imgProcesada = RotateImage(img, 20);
                                            break;
                                        
                                        case 4: // Imagen completa
                                            margenIzqModificado = 0;
                                            margenDerModificado = 0;
                                            margenTopModificado = 0;
                                            margenInfModificado = 0;
                                            break;

                                        case 5: // Márgenes aleatorios (máximo 15% de variación)
                                            Random random = new Random();
                                            margenIzqModificado = margenIzquierdo - random.Next(0, (int)(margenIzquierdo * 0.15));
                                            margenDerModificado = margenDerecho - random.Next(0, (int)(margenDerecho * 0.15));
                                            margenTopModificado = margenSuperior - random.Next(0, (int)(margenSuperior * 0.15));
                                            margenInfModificado = margenInferior - random.Next(0, (int)(margenInferior * 0.15));
                                            break;

                                        case 6: // Rotar imagen -20 grados
                                            imgProcesada = RotateImage(img, -20);
                                            break;
                                    }

                                    using (Image imagenCortada = CropImage(imgProcesada, margenIzqModificado, margenDerModificado, margenTopModificado, margenInfModificado))
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            imagenCortada.Save(ms, ImageFormat.Jpeg);

                                            if (guardarImagenes.ToLower() == "true")
                                            {
                                                imagenCortada.Save(string.Format("{0}/{1}_{2}.jpeg", rutaImagenes, DateTime.Now.ToString("yyyyMMdd_HHmmssfff"), guidRequest), ImageFormat.Jpeg);
                                            }

                                            var results = alpr.Recognize(ms.ToArray());
                                            if (results.results.Any() && results.results.First().candidates.Any())
                                            {
                                                if (results.results.First().candidates.Count > 1)
                                                {
                                                    var patentes = results.results.First().candidates.Select(x => x.plate);
                                                    log.Info(string.Format("Se reconocio más de una patente: ", string.Join(",", patentes.ToArray())));
                                                }

                                                var reconocimiento = results.results.First().candidates.First();
                                                resultado.Patente = reconocimiento.plate.PadRight(12).Trim();
                                                resultado.Confianza = reconocimiento.confidence;
                                                log.Info($"Patente Reconocida - Intento: {intento} - Patente: {resultado.Patente} - {results.ToJson()}");
                                                return resultado;
                                            }
                                        }
                                    }
                                }
                            }
                    }
                }
            }
            return resultado;
        }

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        private static Image CropImage(Image img, int? margenIzq, int? margenDer, int? margenTop, int? margenInf)
        {
            Bitmap bmpImage = new Bitmap(img);

            return bmpImage.Clone(new Rectangle(margenIzq ?? 0, margenTop ?? 0, img.Width - (margenIzq ?? 0) - (margenDer ?? 0), img.Height - (margenTop ?? 0) - (margenInf ?? 0)), bmpImage.PixelFormat);
        }

        private static Image RotateImage(Image img, float angle)
        {
            // Crear un nuevo bitmap para contener la imagen rotada
            Bitmap rotatedBmp = new Bitmap(img.Width, img.Height);
            rotatedBmp.SetResolution(img.HorizontalResolution, img.VerticalResolution);

            // Usar Graphics para aplicar la rotación
            using (Graphics g = Graphics.FromImage(rotatedBmp))
            {
                // Establecer el fondo como transparente
                g.Clear(Color.Transparent);

                // Mover el punto de origen al centro de la imagen
                g.TranslateTransform((float)img.Width / 2, (float)img.Height / 2);

                // Rotar la imagen
                g.RotateTransform(angle);

                // Dibujar la imagen original en el nuevo bitmap
                g.TranslateTransform(-(float)img.Width / 2, -(float)img.Height / 2);
                g.DrawImage(img, new Point(0, 0));
            }

            return rotatedBmp;
        }

    }
}
