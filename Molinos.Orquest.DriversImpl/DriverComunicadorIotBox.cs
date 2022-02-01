using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverComunicadorIotBox : DriverBase, IDriverComunicador, IDriverLogico
    {
        private IDriverItc driverItc;
        private ConfigComunicador configComunicador;
        private string serverTomado;

        private const string MIC = "MIC";
        private const string SPEAKER = "SPEAKER";
        private const string ON = "ON";
        private const string OFF = "OFF";
        private const string FORMATO_COMANDO = "\"{0};{1};{2}\"";

        public string ServerComunicador { get; set; }

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigComunicador); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            configComunicador = (ConfigComunicador)configuracion;
            serverTomado = configComunicador.Dispositivo.Concentrador.TomadoPor.NombreMaquina;
        }

        public override void VerificarDispositivo()
        {
            driverItc.VerificarDispositivo();
        }

        public void ActivarMic()
        {
            Log.Info("Activando Mic: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            var url = string.Format("rtp://{0}:{1}/{2}{3}", Server, configComunicador.PuertoDeAudio, configComunicador.Dispositivo.Codigo, Constantes.IntercomunicadorDireccion.HaciaLaWeb);
            var comando = string.Format(FORMATO_COMANDO, MIC, url, ON);
            var tiempoEjecucion = (configComunicador.TiempoMaximoEjecucion.HasValue) ? configComunicador.TiempoMaximoEjecucion.ToString() : "0";
            driverItc.ActivarSalida(configComunicador.NumeroSalida, comando, tiempoEjecucion, true);
        }

        public void ActivarSpeaker()
        {
            Log.Info("Activando Speaker: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            var url = string.Format("rtsp://{0}/{2}{1}", Server, configComunicador.Dispositivo.Codigo + "-" + configComunicador.PuertoDeAudio, Constantes.IntercomunicadorDireccion.DesdeLaWeb);
            var comando = string.Format(FORMATO_COMANDO, SPEAKER, url, ON);
            var tiempoEjecucion = (configComunicador.TiempoMaximoEjecucion.HasValue) ? configComunicador.TiempoMaximoEjecucion.ToString() : "0";
            driverItc.ActivarSalida(configComunicador.NumeroSalida, comando, tiempoEjecucion, true);
        }

        public void DesactivarMic()
        {
            Log.Info("Desactivando Mic: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            var url = string.Format("rtp://{0}:{1}/{2}{3}", Server, configComunicador.PuertoDeAudio, configComunicador.Dispositivo.Codigo, Constantes.IntercomunicadorDireccion.HaciaLaWeb);
            var comando = string.Format(FORMATO_COMANDO, MIC, url, OFF);
            var tiempoEjecucion = (configComunicador.TiempoMaximoEjecucion.HasValue) ? configComunicador.TiempoMaximoEjecucion.ToString() : "0";
            driverItc.ActivarSalida(configComunicador.NumeroSalida, comando, tiempoEjecucion, true);
        }

        public void DesactivarSpeaker()
        {
            Log.Info("Desactivando Speaker: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            var url = string.Format("rtsp://{0}/{2}{1}", Server, configComunicador.Dispositivo.Codigo + "-" + configComunicador.PuertoDeAudio, Constantes.IntercomunicadorDireccion.DesdeLaWeb);
            var comando = string.Format(FORMATO_COMANDO, SPEAKER, url, OFF);
            var tiempoEjecucion = (configComunicador.TiempoMaximoEjecucion.HasValue) ? configComunicador.TiempoMaximoEjecucion.ToString() : "0";
            driverItc.ActivarSalida(configComunicador.NumeroSalida, comando, tiempoEjecucion, true);
        }

        public void AbrirComunicador()
        {
            Log.Info("Activando Mic y Speaker: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            ActivarMic();
            ActivarSpeaker();
        }

        public void CerrarComunicador()
        {
            Log.Info("Desactivando Mic y Speaker: Comunicador={0}", configComunicador.Dispositivo.Codigo);
            DesactivarMic();
            DesactivarSpeaker();
        }

        public IDriver DriverFisico
        {
            set { driverItc = (IDriverItc)value; }
        }

        private string Server { get => string.IsNullOrEmpty(this.ServerComunicador) ? serverTomado : this.ServerComunicador; }
    }
}