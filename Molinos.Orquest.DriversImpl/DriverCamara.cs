using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCamara : DriverBase, IDriverCamara
    {
        protected string codigoCamara;
        protected ConfigCamara configCamara;
        private IntPtr native_instance;
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCamara); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoCamara = codigo;
            configCamara = (ConfigCamara)configuracion;
        }

        public override void VerificarDispositivo()
        {
            var pingOptions = new PingOptions(128, true);
            using (var ping = new Ping())
            {
                var buffer = new byte[32];
                try
                {
                    var pingReply = ping.Send(new Uri(configCamara.Uri).Host, 3000, buffer, pingOptions);
                    if (pingReply != null)
                    {
                        switch (pingReply.Status)
                        {
                            case IPStatus.Success:
                                break;
                            case IPStatus.TimedOut:
                                throw new ConexionDispositivoDriverException("El intento de conexión dio timeout...");
                            default:
                                throw new ConexionDispositivoDriverException(string.Format("Falló el ping: {0}", pingReply.Status));
                        }
                    }
                    else
                    {
                        throw new ConexionDispositivoDriverException("Falló la conexión por motivo desconocido...");
                    }
                }
                catch (Exception ex)
                {
                    throw new ConexionDispositivoDriverException("Falló la conexión por motivo desconocido...", ex);
                }
            }            
        }

        public virtual ResultadoEjecutar TomarFoto(string filePath, string subPath, string fileName)
        {
            return TomarFoto(filePath, subPath, fileName, false);
        }

        public virtual ResultadoEjecutar TomarFoto(string filePath, string subPath, string fileName, bool retornarImagen)
        {
            //var rand = new Random();
            //var files = Directory.GetFiles("C:\\Img\\Fotos\\20190306\\", "*.jpeg");
            //var a = files[rand.Next(files.Length)];
            //var imagen = File.ReadAllBytes(a);

            byte[] imagen = null;
            var request = (HttpWebRequest)WebRequest.Create(configCamara.Uri);

            if (!string.IsNullOrEmpty(configCamara.NombreUsuario) && !string.IsNullOrEmpty(configCamara.Contrasenia))
            {
                request.Credentials = new NetworkCredential(configCamara.NombreUsuario, Encriptador.Decrypt(configCamara.Contrasenia));
            }

            request.Timeout = configCamara.TimeoutLectura;


            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

            }
            catch (Exception e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoCamara), e);
            }

            if ((response.StatusCode == HttpStatusCode.OK ||
                 response.StatusCode == HttpStatusCode.Moved ||
                 response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using (var inputStream = response.GetResponseStream())
                    {
                        imagen = Procesar(filePath, subPath, fileName, response, inputStream, retornarImagen);
                    }
                }
                catch (IOException e)
                {
                    throw new LecturaEscrituraDriverException(
                        string.Format("El dispositivo {0} no puede escribir en la ruta {1} ", codigoCamara, fileName), e);
                }
            }
            else
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para la url {1}", codigoCamara, configCamara.Uri));
            }
            return new ResultadoTomarFoto
            {
                Imagen = imagen,
                Mensaje = Mensaje.ResultadoOK()
            };
        }

        protected byte[] LeerImagen(Stream inputStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                inputStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        protected void GuardarImagen(string filePath, string subPath, string fileName, HttpWebResponse response, Stream inputStream)
        {
            if (!Directory.Exists(filePath + "\\" + subPath))
            {
                Directory.CreateDirectory(filePath + "\\" + subPath);
            }
            using (
                var outputStream =
                    File.OpenWrite(filePath + "\\" + subPath + "\\" + fileName + "." +
                                   response.ContentType.Substring(
                                       response.ContentType.IndexOf("/", StringComparison.Ordinal) + 1)))
            {
                var buffer = new byte[4096];
                int bytesRead;
                do
                {
                    bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                    outputStream.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }
        }

        protected void GuardarImagen(string filePath, string subPath, string fileName, HttpWebResponse response, byte[] imagen)
        {
            if (!Directory.Exists(filePath + "\\" + subPath))
            {
                Directory.CreateDirectory(filePath + "\\" + subPath);
            }
            using (
                var outputStream =
                    File.OpenWrite(filePath + "\\" + subPath + "\\" + fileName + "." +
                                   response.ContentType.Substring(
                                       response.ContentType.IndexOf("/", StringComparison.Ordinal) + 1)))
            {
                outputStream.Write(imagen, 0, imagen.Length);
            }
        }

        protected virtual byte[] Procesar(string filePath, string subPath, string fileName, HttpWebResponse response, Stream inputStream, bool retornarImagen)
        {
            var imagen = LeerImagen(inputStream);
            if (string.IsNullOrEmpty(filePath) && string.IsNullOrEmpty(subPath) && string.IsNullOrEmpty(fileName))
            {
                return imagen;
            }
            else
            {
                GuardarImagen(filePath, subPath, fileName, response, imagen);
            }

            return retornarImagen ? imagen : null;
        }

    }

}
