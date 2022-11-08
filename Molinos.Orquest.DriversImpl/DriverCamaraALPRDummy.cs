using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.ServicioALPR;
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;
using ResultadoEjecutar = Molinos.Orquest.Dominio.Resultados.ResultadoEjecutar;
using ResultadoObtenerPatente = Molinos.Orquest.Dominio.Resultados.ResultadoObtenerPatente;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCamaraALPRDummy : DriverCamara, IDriverCamara
    {
        public DriverCamaraALPRDummy(IServicioALPR servicioALPR)
        {
            this.servicioALPR = servicioALPR;
        }

        private readonly IServicioALPR servicioALPR;

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
        }

        public override ResultadoEjecutar TomarFoto(string filePath, string subPath, string fileName)
        {
            var resultado = new ResultadoObtenerPatente();
            var rutaFoto = ConfigurationManager.AppSettings["FotoRutaALPRDummy"];
            byte[] imagen = null;
            if (File.Exists(rutaFoto))
            {
                using (var m = new MemoryStream())
                {
                    Image image = Image.FromFile(rutaFoto);
                    image.Save(m, ImageFormat.Jpeg);
                    imagen = m.ToArray();
                }
            }
            else
            {
                Log.Debug($"Foto de ruta ALPR {rutaFoto} no válida");
            }

            resultado.Imagen = imagen;
            resultado.Mensaje = Mensaje.ResultadoOK();
            if (imagen != null && imagen.Length > 0)
            {
                try
                {
                    var resultadoALPR = servicioALPR.LeerPatente(imagen, configCamara.MargenIzquierdo ?? 0, configCamara.MargenDerecho ?? 0, configCamara.MargenSuperior ?? 0, configCamara.MargenInferior ?? 0);
                    resultado.Patente = resultadoALPR.Patente;
                    resultado.Confianza = resultadoALPR.Confianza;
                    if (resultadoALPR.Mensaje != null)
                    {
                        resultado.Mensaje = new Mensaje(resultadoALPR.Mensaje.Codigo, resultadoALPR.Mensaje.Descripcion);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, $"Error al conectarnos al servicio ALPR");
                }
            }
            Log.Info($"Patente reconocida {resultado.Patente} imagen {filePath}\\{subPath}\\{fileName}");
            return resultado;
        }
    }
}