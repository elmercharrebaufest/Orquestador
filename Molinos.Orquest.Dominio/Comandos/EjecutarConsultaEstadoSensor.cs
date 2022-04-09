namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarConsultaEstadoSensor : ComandoEjecutar
    {
        public string[] CodigosDispositivos { get; set; }
        public bool ConsultaParalelo { get; set; }
    }
}
