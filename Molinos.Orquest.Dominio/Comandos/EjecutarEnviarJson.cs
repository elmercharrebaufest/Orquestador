using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarEnviarJson : ComandoEjecutar
    {
        public string json { get; set; }
        public override string ToString()
        {
            return "Ejecutar Enviar Json: " + json + ", IotBox: " + CodigoDispositivo;
        }
    }
}
