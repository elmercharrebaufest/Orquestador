namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarCereoCabezal : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Cereo: " + CodigoDispositivo;
        }
    }
}
