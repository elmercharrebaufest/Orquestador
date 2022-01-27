namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarAperturaCortinaAgua : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Apertura Cortina de Agua: " + CodigoDispositivo;
        }
    }
}
