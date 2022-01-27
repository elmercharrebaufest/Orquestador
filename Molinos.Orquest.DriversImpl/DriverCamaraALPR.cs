using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl.ServicioALPR;
using System;
using Mensaje = Molinos.Orquest.Dominio.Resultados.Mensaje;
using ResultadoEjecutar = Molinos.Orquest.Dominio.Resultados.ResultadoEjecutar;
using ResultadoObtenerPatente = Molinos.Orquest.Dominio.Resultados.ResultadoObtenerPatente;
using ResultadoTomarFoto = Molinos.Orquest.Dominio.Resultados.ResultadoTomarFoto;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCamaraALPR : DriverCamara, IDriverCamara
    {

        public DriverCamaraALPR(IServicioALPR servicioALPR)
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
            
            var imagen = ((ResultadoTomarFoto)base.TomarFoto(filePath, subPath, fileName, true)).Imagen;
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
                catch(Exception e)
                {
                    Log.Error(e, $"Error al conectarnos al servicio ALPR");
                }
            }
            Log.Info($"Patente reconocida {resultado.Patente} imagen {filePath}\\{subPath}\\{fileName}");
            return resultado;
        }

    }

}
