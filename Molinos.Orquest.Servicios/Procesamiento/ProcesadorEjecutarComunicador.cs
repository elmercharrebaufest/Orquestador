using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Enums;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public class ProcesadorEjecutarComunicador : ProcesadorComando<EjecutarComunicador, ResultadoEjecutar>
    {
        public ProcesadorEjecutarComunicador(ILogger log) : base(log)
        {
        }

        protected override ResultadoEjecutar Ejecutar(EjecutarComunicador comando, Dispositivo dispositivo, IDriver driver)
        {
            var d = ((IDriverComunicador)driver);
            d.ServerComunicador = comando.ServerComunicador;

            switch (comando.Tipo)
            {
                case TipoComunicador.Speaker:
                    if (comando.Activar) d.ActivarSpeaker(); else d.DesactivarSpeaker();
                    break;
                case TipoComunicador.Mic:
                    if (comando.Activar) d.ActivarMic(); else d.DesactivarMic();
                    break;
                case TipoComunicador.Both:
                    if (comando.Activar) d.AbrirComunicador(); else d.CerrarComunicador();
                    break;
                default:
                    break;
            }

            return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };
        }
    }
}
