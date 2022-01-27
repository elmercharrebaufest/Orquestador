using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molinos.Orquest.Drivers
{
    public interface IDriverComunicador : IDriver
    {
        string ServerComunicador { get; set; }
        void ActivarMic();
        void ActivarSpeaker();
        void DesactivarMic();
        void DesactivarSpeaker();
        void AbrirComunicador();
        void CerrarComunicador();
    }
}
