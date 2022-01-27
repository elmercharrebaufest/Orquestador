using System;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarAnalisisHumedad : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Análisis Humedad: " + CodigoDispositivo;
        }

        public DateTime FechaDeInicio { get; set; }
    }
}
