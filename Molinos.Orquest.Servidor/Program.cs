using System.ServiceProcess;
using System.Windows.Forms;

namespace Molinos.Orquest.Servidor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            var service = new ServicioWindows();
            try
            {
                service.Start(new string[0]);
                MessageBox.Show("Presione 'OK' para detener el servicio.", "Orquestador", MessageBoxButtons.OK);
            }
            finally
            {
                service.Stop();
            }
#else
            ServiceBase.Run(new ServicioWindows());
#endif
        }
    }
}
