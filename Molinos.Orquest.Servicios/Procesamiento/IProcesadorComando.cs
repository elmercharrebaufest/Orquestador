using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.Servicios.Procesamiento
{
    public interface IProcesadorComando
    {
        ResultadoComando Procesar(Comando comando, Dispositivo dispositivo, IDriver driver);
    }

    public interface IProcesadorComando<TComando, TResultado> : IProcesadorComando where TComando : Comando where TResultado: ResultadoComando
    {
        TResultado Procesar(TComando comando, Dispositivo dispositivo, IDriver driver);
    }
}
