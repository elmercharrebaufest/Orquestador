using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverImpresoraHasarContinuo : DriverBase, IDriverImpresoraHasar
    {
        private string codigoImpresoraHasar;
        private ConfigImpresoraHasar configImpresoraHasar;
        private TcpCommandClient cliente;
        private bool conectado;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigImpresoraHasar); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoImpresoraHasar = codigo;
            configImpresoraHasar = (ConfigImpresoraHasar)configuracion;
            cliente = new TcpCommandClient(configImpresoraHasar.DireccionIp, configImpresoraHasar.Puerto, 30, configImpresoraHasar.TimeoutLectura, Log);
        }

        public override void VerificarDispositivo()
        {
            if (cliente == null || !cliente.Conectado)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", configImpresoraHasar));
            }
        }

        public override bool MantenerConectado()
        {
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cliente.Dispose();
            }
        }

        public void ImprimirTicket(EjecutarImpresionTicket comando)
        {
            ImprimirTicketRecursivo(comando, 5);
        }

        private void ImprimirTicketRecursivo(EjecutarImpresionTicket comando, int intentos)
        {
            try
            {
                if (!cliente.Conectado || !conectado)
                {
                    cliente.ReConectar();
                    conectado = true;
                }
                Log.Error($"Conecto con exito. Imprimiendo");

                foreach (var imprimir in comando.Ticket)
                {

                    var array = Encoding.ASCII.GetBytes(imprimir);
                    cliente.EnviarComando(array);
                    LineasVacias(cliente, 1);
                }
                LineasVacias(cliente, 5);
                Cortar(cliente);
                Log.Debug($"Fin Impresion");
            }
            catch (Exception e)
            {
                Log.Error(e, "Falló la conexión al dispositivo {0}", codigoImpresoraHasar);
                if (intentos == 0)
                {
                    throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoImpresoraHasar), e);
                }
                conectado = false;
                Thread.Sleep(500);
                ImprimirTicketRecursivo(comando, intentos - 1);
            }
        }

        private void LineasVacias(TcpCommandClient cliente, int lineas)
        {
            for (int i = 0; i < lineas; i++)
            {
                cliente.EnviarComando(new byte[] { 0x0A });
            }
        }
        private void Cortar(TcpCommandClient cliente)
        {
            cliente.EnviarComando(new byte[] { 29, 86, 01 });

        }
    }
}
