namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarEstacionMeteorologica : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Capturar Información Meteorológica: " + CodigoDispositivo;
        }
    }
}
