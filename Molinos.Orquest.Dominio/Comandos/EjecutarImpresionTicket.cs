using System.Collections.Generic;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarImpresionTicket : ComandoEjecutar
    {
        public List<string> Ticket { get; set; }
        public override string ToString()
        {
            return "Ejecutar Impimir Ticket: " + CodigoDispositivo;
        }
    }
}
