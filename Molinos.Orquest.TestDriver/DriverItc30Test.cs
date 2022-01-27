using System.Collections.Generic;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using NUnit.Framework;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    public class DriverItc30Test
    {
        private string codigoItc = "ITCEM01";
        private ConfigItc configItc;

        [SetUp]
        public void SetUp()
        {
            configItc = new ConfigItc
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverItc30, Molinos.Orquest.DriversImpl",
                DireccionIp = "192.168.34.7",
                Puerto = 1470,
                LongFrase = 50,
                TimeoutLectura = 10000,
                IntervaloPolling = 1000,
                CarInicioFrase = "<",
                CarFinFrase = ">",
                DelimitadorCampos = " ",
                ComandoEstado = "R",
                ComandoTarjeta = "U",
                ComandoActivarSalida = "S",
                RespuestaExito = "A",
                RespuestaError = "F"
            };
        }

        [Test]
        public void TestEventoLecturaTarjeta()
        {
            var tarjetasLeidas = new List<string>();
            var driver = new DriverItc30();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoItc, configItc);
            driver.EventoDriver += (sender, args) =>
                {
                    lock (tarjetasLeidas)
                    {
                        if (args.Notificacion.CodigoEvento == CodigosEventos.LecturaTarjetaRecibida)
                        {
                            tarjetasLeidas.Add(args.Notificacion.Datos["Tarjeta"]);
                        }
                    }
                };
            var continuar = true;
            while (continuar)
            {
                lock (tarjetasLeidas)
                {
                    continuar = tarjetasLeidas.Count < 4;
                }
                Thread.Sleep(100);
            }
            Assert.That(tarjetasLeidas, Has.Count.EqualTo(4));
            Assert.That(tarjetasLeidas[0], Is.EqualTo("0001504523"));
            Assert.That(tarjetasLeidas[1], Is.EqualTo("0008622089"));
            Assert.That(tarjetasLeidas[2], Is.EqualTo("0008622089"));
            Assert.That(tarjetasLeidas[3], Is.EqualTo("0001504523"));
        }

        [Test]
        public void TestEventoLecturaMismaTarjeta()
        {
            var tarjetasLeidas = new List<string>();
            var driver = new DriverItc30();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoItc, configItc);
            driver.EventoDriver += (sender, args) =>
            {
                lock (tarjetasLeidas)
                {
                    tarjetasLeidas.Add(args.Notificacion.Datos["Tarjeta"]);
                }
            };
            var continuar = true;
            while (continuar)
            {
                lock (tarjetasLeidas)
                {
                    continuar = tarjetasLeidas.Count < 4;
                }
                Thread.Sleep(100);
            }
            Assert.That(tarjetasLeidas, Has.Count.EqualTo(4));
            Assert.That(tarjetasLeidas[0], Is.EqualTo("0001504523"));
            Assert.That(tarjetasLeidas[1], Is.EqualTo("0001504523"));
            Assert.That(tarjetasLeidas[2], Is.EqualTo("0001504523"));
            Assert.That(tarjetasLeidas[3], Is.EqualTo("0001504523"));
        }

        [Test]
        public void TestActivarSalida()
        {
            var driver = new DriverItc30();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoItc, configItc);

            for (int i = 1; i < 100; i++)
            {
                driver.ActivarSalida(1, "true", "2");
                Thread.Sleep(30);
                driver.ActivarSalida(2, "true", "2");
                Thread.Sleep(30);
                driver.ActivarSalida(3, "true", "2");
                Thread.Sleep(30);
                driver.ActivarSalida(4, "true", "2");
                Thread.Sleep(100);
            }
        }

        [Test]
        public void TestLecturaEntrada()
        {
            var activadas = new List<string>();
            var desactivadas = new List<string>();
            var driver = new DriverItc30();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoItc, configItc);
            driver.EventoDriver += (sender, args) =>
            {
                if (args.Notificacion.CodigoEvento == CodigosEventos.EntradaActivada)
                {
                    lock (activadas)
                    {
                        activadas.Add(args.Notificacion.Datos["Entrada"]);
                    }
                } 
                else if (args.Notificacion.CodigoEvento == CodigosEventos.EntradaDesactivada)
                {
                    lock (desactivadas)
                    {
                        desactivadas.Add(args.Notificacion.Datos["Entrada"]);
                    }
                }

            };
            var continuar = true;
            while (continuar)
            {
                lock (activadas)
                {
                    lock (desactivadas)
                    {
                        continuar = activadas.Count < 4 || desactivadas.Count < 4;
                    } 
                }
                Thread.Sleep(100);
            }
            Assert.That(activadas, Has.Count.EqualTo(4));
            Assert.That(activadas[0], Is.EqualTo("1"));
            Assert.That(activadas[1], Is.EqualTo("2"));
            Assert.That(activadas[2], Is.EqualTo("3"));
            Assert.That(activadas[3], Is.EqualTo("4"));

            Assert.That(desactivadas, Has.Count.EqualTo(4));
            Assert.That(desactivadas[0], Is.EqualTo("4"));
            Assert.That(desactivadas[1], Is.EqualTo("3"));
            Assert.That(desactivadas[2], Is.EqualTo("2"));
            Assert.That(desactivadas[3], Is.EqualTo("1"));
        }

        [Test]
        public void TestLecturaEntradaActivadaDesactivada()
        {
            var activadas = new List<string>();
            var desactivadas = new List<string>();
            var driver = new DriverItc30();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoItc, configItc);
            driver.EventoDriver += (sender, args) =>
            {
                if (args.Notificacion.CodigoEvento == CodigosEventos.EntradaActivada)
                {
                    lock (activadas)
                    {
                        activadas.Add(args.Notificacion.Datos["Entrada"]);
                    }
                }
                else if (args.Notificacion.CodigoEvento == CodigosEventos.EntradaDesactivada)
                {
                    lock (desactivadas)
                    {
                        desactivadas.Add(args.Notificacion.Datos["Entrada"]);
                    }
                }

            };
            var continuar = true;
            while (continuar)
            {
                lock (activadas)
                {
                    lock (desactivadas)
                    {
                        continuar = activadas.Count < 1 || desactivadas.Count < 1;
                    }
                }
                Thread.Sleep(100);
            }
            Assert.That(activadas, Has.Count.EqualTo(1));
            Assert.That(activadas[0], Is.EqualTo("1"));
            Assert.That(desactivadas, Has.Count.EqualTo(1));
            Assert.That(desactivadas[0], Is.EqualTo("1"));
        }


    }
}
