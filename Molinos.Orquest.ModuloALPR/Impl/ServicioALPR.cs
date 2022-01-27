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
                    using (MemoryStream mStream = new MemoryStream(imagen))
                    {
                        using (Image img = Image.FromStream(mStream))
                        {
                            using (Image imagenCortada = CropImage(img, margenIzquierdo, margenDerecho, margenSuperior, margenInferior))
                            {
                                using (var ms = new MemoryStream())
                                {
                                    imagenCortada.Save(ms, ImageFormat.Jpeg);
                                    var results = alpr.Recognize(ms.ToArray());
                                    if (results.results.Any() && results.results.First().candidates.Any())
                                    {
                                        var reconocimiento = results.results.First().candidates.First();
                                        resultado.Patente = reconocimiento.plate.PadRight(12).Trim();
                                        resultado.Confianza = reconocimiento.confidence;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            log.Info($"Patente reconocida {resultado.Patente} ");
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

        

    }
}
