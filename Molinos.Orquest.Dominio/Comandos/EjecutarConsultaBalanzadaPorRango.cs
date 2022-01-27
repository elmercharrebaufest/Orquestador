namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarConsultaBalanzadaPorRango : ComandoEjecutar
    {
        public int IdBalanzadaInicio { get; set; }
        public int IdBalanzadaFin { get; set; }
        public override string ToString()
        {
            return "Ejecutar Consulta de Balanzadas por rango. Inicio: " + IdBalanzadaInicio + " - Fin: " + IdBalanzadaFin + ". CodigoDispositivo: " + CodigoDispositivo;
        }
    }
}
