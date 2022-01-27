using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Resultados;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public interface IProcesadorDispositivo : IDisposable
    {
        string CodigoDispositivo { get; }
        ResultadoComando Procesar(Comando comando);
        bool ProcesarRemanentes();
    }
}
