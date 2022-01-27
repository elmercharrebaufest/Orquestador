namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarBorrarBalanzada : ComandoEjecutar
    {
        public int IdBorrado { get; set; }
        public override string ToString()
        {
            return "Ejecutar Borrar Balanzada: " + IdBorrado + ", Balanza: " + CodigoDispositivo;
        }
    }
}
