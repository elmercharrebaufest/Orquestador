namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarAperturaBarrera : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Apertura Barrera: " + CodigoDispositivo;
        }
    }
}
