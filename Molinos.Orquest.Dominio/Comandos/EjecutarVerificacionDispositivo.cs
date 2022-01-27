namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarVerificacionDispositivo : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Verificación Dispositivo: " + CodigoDispositivo;
        }
    }
}
