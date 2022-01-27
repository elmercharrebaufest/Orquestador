namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarCierreBarrera : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Apertura Barrera: " + CodigoDispositivo;
        }
    }
}
