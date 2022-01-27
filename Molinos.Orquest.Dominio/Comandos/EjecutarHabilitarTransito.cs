using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Comandos
{
    //TODO: Deprecar
    public class EjecutarHabilitarTransito : ComandoEjecutar
    {
        public string Direccion { get; set; }
        public string Tarjeta { get; set; }
        public int? FichadaId { get; set; }
        public override string ToString()
        {
            return "Ejecutar Habilitar Molinete: " + CodigoDispositivo;
        }
    }
}
