using System;
using System.Collections.Generic;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverCartelLed : IDriver
    {
        void EnviarMensaje(string texto, string numeroPrograma, string numeroTrama, string numeroVariable);
    }
}
