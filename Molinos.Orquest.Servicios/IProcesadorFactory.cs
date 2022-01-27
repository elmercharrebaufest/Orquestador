using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Servicios.Procesamiento;

namespace Molinos.Orquest.Servicios
{
    public interface IProcesadorFactory
    {
        IProcesadorComando ProcesadorPara(Comando comando);
        IProcesadorDispositivo ProcesadorParaDispositivo(Dispositivo dispositivo, Action<IProcesadorDispositivo> callBackFinProcesamiento);
    }
}
