namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarPesaje : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Pesaje: " + CodigoDispositivo;
        }
    }
}
