using System;

namespace Molinos.Orquest.Dominio.Comandos
{
    public class EjecutarObtenerDireccionViento : ComandoEjecutar
    {
        public override string ToString()
        {
            return "Ejecutar Direccion del Viento: " + CodigoDispositivo;
        }

       
    }
}
