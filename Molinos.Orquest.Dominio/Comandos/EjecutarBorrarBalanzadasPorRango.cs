namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarBorrarBalanzadasPorRango : ComandoEjecutar
    {
        public int IdBalanzadaInicio { get; set; }
        public int IdBalanzadaFin { get; set; }
        public override string ToString()
        {
            return "Ejecutar Borrado de Balanzadas por rango. Inicio: " + IdBalanzadaInicio + " - Fin: " + IdBalanzadaFin + ". CodigoDispositivo: " + CodigoDispositivo;
        }
    }
}
