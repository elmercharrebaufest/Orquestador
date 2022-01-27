namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarTomarFoto : ComandoEjecutar
    {
        public string FilePath { get; set; }
        public string SubPath { get; set; }
        public string FileName { get; set; }
        public override string ToString()
        {
            return "Ejecutar Tomar Foto: " + CodigoDispositivo;
        }
    }
}
