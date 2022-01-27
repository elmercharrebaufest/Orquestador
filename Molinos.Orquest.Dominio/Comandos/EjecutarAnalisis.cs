using System;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarAnalisis : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Análisis: " + CodigoDispositivo;
        }

        public string Material { get; set; }
        public string IdentificadorDeMuestra { get; set; }
    }
}
