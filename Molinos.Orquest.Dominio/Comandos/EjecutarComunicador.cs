using Molinos.Orquest.Dominio.Enums;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarComunicador : ComandoEjecutar
    {
        public bool Activar { get; set; }
        public TipoComunicador Tipo { get; set; }
        public string ServerComunicador { get; set; }
        public override string ToString()
        {
            return "Ejecutar Activar Comunicador: " + CodigoDispositivo;
        }
    }
}
