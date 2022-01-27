namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarAperturaBarreraMaestro : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Apertura Barrera Maestro: " + CodigoDispositivo;
        }
    }
}
