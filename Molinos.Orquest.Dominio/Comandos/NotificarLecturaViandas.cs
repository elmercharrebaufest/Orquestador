namespace Molinos.Orquest.Dominio.Comandos
{
    public class NotificarLecturaViandas : ComandoEjecutar
    {
        public string Tarjeta { get; set; }

        public override string ToString()
        {
            return "Ejecutar NotificarLecturaViandas: " + CodigoDispositivo;
        }
    }
}
