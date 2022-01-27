using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Dominio.Seguridad;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Atributos;
using Molinos.Orquest.Web.Models;
using Ninject.Extensions.Logging;
using Ninject;


namespace Molinos.Orquest.Web.Controllers
{
    [Autorizacion(PermisosOrquestador.PruebaItc, PermisosOrquestador.PruebaCabezal, PermisosOrquestador.PruebaHumedimetro)]
    public class PruebaConexionController : BaseController
    {
        private IDriverFactory driverFactory;
        public PruebaConexionController(IRepositorioFactory repositorioFactory, IServicioOrquestador servicioOrquestador, ILogger logger, IDriverFactory driverFactory)
            : base(repositorioFactory, servicioOrquestador, logger)
        {

            this.driverFactory = driverFactory;
        }

        public ActionResult Index(string ip, int puerto,string codigo)
        {
            ViewBag.IpDispositivo = ip;
            ViewBag.PuertoDispositivo = puerto;
            ViewBag.Codigo = codigo;
            return View();
        }

        public ActionResult Ping(string ip)
        {
            var resultado = new List<ResultadoPruebaModel>();
            var pingOptions = new PingOptions(128, true);
            using (var ping = new Ping())
            {
                var buffer = new byte[32];
            
                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        var pingReply = ping.Send(ip, 3000, buffer, pingOptions);

                        if (pingReply != null)
                        {
                            switch (pingReply.Status)
                            {
                                case IPStatus.Success:
                                    resultado.Add(new ResultadoPruebaModel(string.Format("Respuesta de {0}: bytes={1} tiempo={2}ms TTL={3}",
                                                                                         pingReply.Address, pingReply.Buffer.Length,
                                                                                         pingReply.RoundtripTime, (pingReply.Options != null) ? pingReply.Options.Ttl : (int?)null), false));
                                    break;
                                case IPStatus.TimedOut:
                                    resultado.Add(new ResultadoPruebaModel("El intento de conexión dio timeout...", true));
                                    break;
                                default:
                                    resultado.Add(new ResultadoPruebaModel(string.Format("Falló el ping: {0}", pingReply.Status), true));
                                    break;
                            }
                        }
                        else
                        {
                            resultado.Add(new ResultadoPruebaModel("Falló la conexión por motivo desconocido...", true));
                        }
                    }
                    catch (PingException ex)
                    {
                        resultado.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
                    }
                    catch (SocketException ex)
                    {
                        resultado.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
                    }
                }
            }

            return View("ResultadoPrueba", resultado);
        }

        public ActionResult Loopback(string ip, int puerto, int repeticiones)
        {
            var resultados = new List<ResultadoPruebaModel>(); 
            var datos = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16};
            try
            {
                using (var tcpClient = new TcpClient(ip, puerto))
                {
                    tcpClient.ReceiveBufferSize = datos.Length;
                    tcpClient.SendBufferSize = datos.Length;
                    var stream = tcpClient.GetStream();

                    for (var i = 0; i < repeticiones; i++)
                    {
                        try
                        {
                            var datosLeidos = new byte[datos.Length];
                            stream.Write(datos, 0, datos.Length);
                            stream.Read(datosLeidos, 0, datosLeidos.Length);

                            resultados.Add(
                                SonIguales(datos, datosLeidos)
                                    ? new ResultadoPruebaModel(Textos.PruebaExitosa, false)
                                    : new ResultadoPruebaModel(Textos.PruebaFallida, true));
                        }
                        catch (Exception ex)
                        {
                            resultados.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resultados.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
            }
            
            return View("ResultadoPrueba", resultados);
        }

        private static bool SonIguales(byte[] datos, byte[] datosLeidos)
        {
            var iguales = true;
            for (var i = 0; i < datos.Length && iguales; i++)
            {
                iguales = datos[i] == datosLeidos[i];
            }
            return iguales;
        }
        public ActionResult HistTest(string codigo,string query )
        {
            //log.Debug("Empezamos con el test al historian");
            //log.Debug(codigo);
            //log.Debug(query);
            //ResultadoEjecutarQuery result = new ResultadoEjecutarQuery();
                        
            //var dispositivo = repositorio.Obtener<Dispositivo>(s => s.Codigo == codigo);
            //var tag = driverFactory.Driver<IDriverTag>(dispositivo);
            //var resultados = new List<ResultadoPruebaModel>();

            //log.Debug(query);

            //try
            //{
            //    log.Debug("Llamada al driver");

            //     result = tag.EjecutarQuery(query);
                
            //    for (int i = 0; i < result.queryResult.Count; i++)
            //    {
            //        resultados.Add(new ResultadoPruebaModel(result.queryResult[i], false));
            //    }
            //}
            //catch (Exception ex)
            //{
            //    log.Error(ex.Message + "{0} + {1}", Historian.ConectionString, tag.TipoDispositivo);
            //    log.Debug(ex.Message, Historian, tag);
            //    resultados.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));

            //}

            /*
            
            


            try
            {

                var connection = new SqlConnection(Historian.ConectionString);
                var command = new SqlCommand(query, connection);
            
                try
                {
                    connection.Open();
                    Console.WriteLine("Connected");
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        resultados.Add(new ResultadoPruebaModel(reader.GetFieldValue<string>(0) + " - " + reader.GetFieldValue<string>(1) + " - " +
                            reader.GetFieldValue<string>(2) + " - " + reader.GetFieldValue<string>(3) + " - " + reader.GetFieldValue<string>(4) + " - " +
                            reader.GetFieldValue<string>(5) + " - " + reader.GetFieldValue<string>(6) + " - " +
                            reader.GetFieldValue<string>(7) + " - " + reader.GetFieldValue<string>(8), false));
                    }
                }
                catch (Exception ex)
                {
                    resultados.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
                }
            }
            catch (Exception ex)
            {

                resultados.Add(new ResultadoPruebaModel(string.Format(Textos.ErrorDeConexion, ex.Message), true));
            } */

            return View("ResultadoPrueba",null);
        }
    }
}
