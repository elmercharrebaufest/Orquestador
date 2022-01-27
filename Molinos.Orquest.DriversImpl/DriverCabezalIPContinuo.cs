using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCabezalIPContinuo : DriverBase, IDriverCabezal
    {
        private string codigoCabezal;
        private ConfigCabezal configuracionCabezal;

        public override Type TipoDispositivo
        {
            get { return typeof (ConfigCabezal); }
        }

        public override void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            codigoCabezal = codigo;
            configuracionCabezal = (ConfigCabezal) configuracion;
        }

        public override void VerificarDispositivo()
        {
            ObtenerPeso();
        }

        public decimal? ObtenerPeso()
        {
            try
            {
                using (var cliente = new TcpCommandClient(configuracionCabezal.DireccionIp, configuracionCabezal.Puerto, configuracionCabezal.LongFrase * 2, configuracionCabezal.TimeoutLectura,Log))
                {
                    var lecturasEstables = 1;
                    var pesoAnterior = LeerPeso(cliente);
                    for (var i = 1; i < configuracionCabezal.MaxCantLecPesoEstable && lecturasEstables < configuracionCabezal.CantLecPesoEstable; i++)
                    {
                        Thread.Sleep(configuracionCabezal.IntLecPesoEstable);
                        var peso = LeerPeso(cliente);
                        lecturasEstables = pesoAnterior == peso ? lecturasEstables + 1 : 0;
                        pesoAnterior = peso;
                    }
                    return (lecturasEstables == configuracionCabezal.CantLecPesoEstable) ? pesoAnterior : (decimal?)null;
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoCabezal), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoCabezal), e);
            }
            catch (FormatException e)
            {
                throw new FormatoRespuestaDriverException(string.Format("Formato de respuesta del dispositivo {0} incorrecto para el comando {1}", codigoCabezal, configuracionCabezal.ComandoPeso), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoCabezal), e);
            }
        }

        public bool ForzarCero()
        {
            try
            {
                using (var cliente = new TcpCommandClient(configuracionCabezal.DireccionIp, configuracionCabezal.Puerto, configuracionCabezal.LongFrase * 2, configuracionCabezal.TimeoutLectura, Log))
                {
                    Log.Info("Cereo de Balanza - " + configuracionCabezal.Dispositivo.Codigo);
                    cliente.EnviarComando(configuracionCabezal.ComandoCereo);
                    Thread.Sleep(configuracionCabezal.IntLecCereo);
                    var peso = LeerPeso(cliente);
                    return peso == 0M;
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoCabezal), e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(string.Format("Falló la conexión al dispositivo {0}", codigoCabezal), e);
            }
            catch (Exception e)
            {
                throw new DriverException(string.Format("Error al conectarse al dispositivo {0}", codigoCabezal), e);
            }
        }

        private decimal? LeerPeso(TcpCommandClient cliente)
        {
            Log.Debug("LeerPeso");
            decimal? peso = null;
            int attempt = 0;
            string respuesta = null;
            while (respuesta == null && attempt < 10)
            {
                
                respuesta = cliente.LeerRespuestaContinua(
                                configuracionCabezal.CarInicioFrase.Split(',').Select(Char.Parse).ToArray(),
                                configuracionCabezal.PosDesde, 
                                configuracionCabezal.PosHasta);
                Log.Info("Captura de Peso - " + configuracionCabezal.Dispositivo.Codigo + " - '" + (respuesta != null ? respuesta.Replace("\r", "") : "No Responde") + "'");
                Log.Debug("LeerPeso attempt{0}, respuesta {1}", attempt, respuesta);
                attempt++;
            }
            if (respuesta != null)
            {
                
                peso = Convert.ToDecimal(respuesta.Trim());
                peso = peso / new Decimal(Math.Pow(10, configuracionCabezal.DigitosDecimales));
            }
            return peso;
        }
    }
}
