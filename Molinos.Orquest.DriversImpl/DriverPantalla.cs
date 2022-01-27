using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverPantalla : DriverBase, IDriverPantalla
    {
        private readonly object lockComando = new object();

        private bool notificaEventos = false;
        private string codigoItc;
        private ConfigPantalla configPantalla;
        private readonly ManualResetEvent finCiclo = new ManualResetEvent(false);
        private bool? falloUltimaConexion;
        private Exception errorUltimaConexion;
        private Image ultimaImagen;
        
        public override Type TipoDispositivo
        {
            get { return typeof(ConfigPantalla); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoItc = codigo;
            configPantalla = (ConfigPantalla)configuracion;


            notificaEventos = true;

            Log.Debug("Iniciando Driver de Pantalla {0}", codigo);
            Task.Run(() =>
            {
                while (notificaEventos)
                {
                    try
                    {
                        DescargarImagen();

                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error al DescargarImagen del Pantalla {0}", codigoItc);

                        if (!falloUltimaConexion.HasValue || !falloUltimaConexion.Value)
                        {
                            Log.Info("Desconexión de Pantalla={0}", codigoItc);
                            falloUltimaConexion = true;
                            errorUltimaConexion = e;
                        }

                    }
                    Thread.Sleep(configPantalla.TiempoDeRefresco * 1000);
                }
                finCiclo.Set();
            });
        }
        private void DescargarImagen()
        {

            var request = (HttpWebRequest)WebRequest.Create(configPantalla.UrlSCATO);

            request.UseDefaultCredentials = true;

            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.GetResponse();

            }
            catch (Exception e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión a la Pantalla de SCATO{0}", configPantalla.Dispositivo.Codigo), e);
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
                        var imagen = ReadFully(inputStream);
                        ultimaImagen = (Bitmap)((new ImageConverter()).ConvertFrom(imagen));
                        FtpWebRequest r = (FtpWebRequest)WebRequest.Create(configPantalla.UrlFTP);
                        r.Proxy = new WebProxy();
                        r.KeepAlive = false;
                        r.Credentials = new NetworkCredential(configPantalla.NombreUsuario, configPantalla.Contrasenia);
                        r.Method = WebRequestMethods.Ftp.UploadFile;
                        
                        using (Stream ftpStream = r.GetRequestStream())
                        {
                            ftpStream.Write(imagen, 0, imagen.Length);
                            ftpStream.Close();
                        }
                        inputStream.Close();
                    }
                    falloUltimaConexion = false;
                    errorUltimaConexion = null;

                }
                catch (IOException e)
                {
                    throw new LecturaEscrituraDriverException(
                        string.Format("La Pantalla {0} no puede escribir en la ruta {1} ", configPantalla.Dispositivo.Codigo, configPantalla.UrlFTP), e);
                }
            }
            else
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para la url {1}", configPantalla.Dispositivo.Codigo, configPantalla.UrlSCATO));
            }
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public override void VerificarDispositivo()
        {
            if (falloUltimaConexion.HasValue && falloUltimaConexion.Value)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión a la Pantalla de SCATO{0}", configPantalla.Dispositivo.Codigo));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                notificaEventos = false;
                finCiclo.WaitOne();
                finCiclo.Dispose();
            }
        }
        public override bool MantenerConectado()
        {
            return true;
        }

        public virtual ResultadoEjecutar ObtenerUltimaFoto()
        {
            byte[] imagen = null;
            if (ultimaImagen != null)
            {
                imagen = ImageToByteArray(ultimaImagen);
            }

            return new ResultadoTomarFoto
            {
                Imagen = imagen,
                Mensaje = (falloUltimaConexion.HasValue && !falloUltimaConexion.Value) ? Mensaje.ResultadoOK() : new Mensaje(Codigos.ErrorDriver, errorUltimaConexion==null?Textos.Error:errorUltimaConexion.Message)
            };
        }

        public byte[] ImageToByteArray(System.Drawing.Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                imageIn.Save(ms, imageIn.RawFormat);
                return ms.ToArray();
            }
        }
    }
}
