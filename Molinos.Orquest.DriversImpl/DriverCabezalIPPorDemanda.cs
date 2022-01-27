using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;

namespace Molinos.Orquest.DriversImpl
{
    public class DriverCabezalIPPorDemanda : DriverBase, IDriverCabezal
    {
        private string codigoCabezal;
        private ConfigCabezal configuracionCabezal;

        public override Type TipoDispositivo
        {
            get { return typeof(ConfigCabezal); }
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
                using (var cliente = new TcpCommandClient(configuracionCabezal.DireccionIp, configuracionCabezal.Puerto, configuracionCabezal.LongFrase, configuracionCabezal.TimeoutLectura, Log))
                {
                    var lecturasEstables = 1;
                    var pesoAnterior = EnviarComandoPeso(cliente);
                    for (var i = 1; i < configuracionCabezal.MaxCantLecPesoEstable && lecturasEstables < configuracionCabezal.CantLecPesoEstable; i++)
                    {
                        Thread.Sleep(configuracionCabezal.IntLecPesoEstable);
                        var peso = EnviarComandoPeso(cliente);
                        lecturasEstables = pesoAnterior == peso ? lecturasEstables + 1 : 0;
                        pesoAnterior = peso;
                    }
                    return (lecturasEstables == configuracionCabezal.CantLecPesoEstable) ? pesoAnterior : (decimal?) null;
                }
            }
            catch (SocketException e)
            {
                throw new ConexionDispositivoDriverException(" Posible falla: equipo conversor apagado o ausencia de link de red. Por favor comunicarse con Mantenimiento Electronico", e);
            }
            catch (IOException e)
            {
                throw new ConexionDispositivoDriverException(" Posible falla: cabezal de balanza o cableado serie. Por favor comuinicarse con Mantenimiento Electronico", e);
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
                using (var cliente = new TcpCommandClient(configuracionCabezal.DireccionIp, configuracionCabezal.Puerto, configuracionCabezal.LongFrase, configuracionCabezal.TimeoutLectura, Log))
                {
                    Log.Info("Cereo de Balanza - " + configuracionCabezal.Dispositivo.Codigo);
                    cliente.EnviarComando(configuracionCabezal.ComandoCereo);
                    Thread.Sleep(configuracionCabezal.IntLecCereo);
                    var peso = EnviarComandoPeso(cliente);
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

        private decimal EnviarComandoPeso(TcpCommandClient cliente)
        {
            var respuesta = cliente.EnviarComando(configuracionCabezal.ComandoPeso, configuracionCabezal.PosDesde, configuracionCabezal.PosHasta);
            Log.Info("Captura de Peso - " + configuracionCabezal.Dispositivo.Codigo + " - '" + (respuesta != null ? respuesta.Replace("\r", "") : "No Responde") + "'");
            var peso = Convert.ToDecimal(respuesta.Trim());
            peso = peso / new Decimal(Math.Pow(10, configuracionCabezal.DigitosDecimales));
            return peso;
        }
       

    }
}
