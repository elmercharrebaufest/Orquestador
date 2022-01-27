using System;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class ComandoDepurarSuscripciones : Comando
    {
        public DateTime Vencimiento { get; set; }
        public bool DepurarPersistentes { get; set; }


        public override string ToString()
        {
            return string.Format("Depurar Suscripciones {0} Vencimiento: {1} DepurarPersistentes: {2}",
                CodigoDispositivo, Vencimiento, DepurarPersistentes);
        }
    }
}
