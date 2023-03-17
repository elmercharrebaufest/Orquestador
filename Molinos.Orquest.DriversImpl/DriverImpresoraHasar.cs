using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverImpresoraHasar : DriverBase, IDriverImpresoraHasar
    {
        private string codigoImpresoraHasar;
        private ConfigImpresoraHasar configuracionImpresoraHasar;

        public override Type TipoDispositivo
        {
            get { return typeof (ConfigImpresoraHasar); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoImpresoraHasar = codigo;
            configuracionImpresoraHasar = (ConfigImpresoraHasar) configuracion;
        }

        public override void VerificarDispositivo()
        {
        }

        public void ImprimirTicket(EjecutarImpresionTicket comando)
        {
            try
            {
                Log.Info($"Conectando a { configuracionImpresoraHasar.DireccionIp }");
                using (var cliente = new TcpCommandClient(configuracionImpresoraHasar.DireccionIp, configuracionImpresoraHasar.Puerto, 30, configuracionImpresoraHasar.TimeoutLectura, Log))
                {
                    Log.Debug($"Conecto con exito. Imprimiendo");
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
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoImpresoraHasar), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoImpresoraHasar), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoImpresoraHasar), e);
            }
        }
        private void LineasVacias(TcpCommandClient cliente, int lineas)
        {
            for(int i=0; i < lineas; i++)
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
