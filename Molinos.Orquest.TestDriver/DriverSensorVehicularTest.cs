using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    public class DriverSensorVehicularTest
    {
        private string codigoSensor = "SENSOR_VEH_01";
        private ConfigSensor configSensor;
        private Mock<IDriverItc> mockDriverItc;

        [SetUp]
        public void SetUp()
        {
            configSensor = new ConfigSensor
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverSensorVehicular, Molinos.Orquest.DriversImpl",
                NumeroEntrada = 3,
                Camara = new ConfigCamara
                {
                    Uri = "http://192.168.1.100/snapshot.jpg",
                    TimeoutLectura = 5000,
                    NombreUsuario = "",
                    Contrasenia = "",
                    MargenIzquierdo = 0,
                    MargenDerecho = 0,
                    MargenSuperior = 0,
                    MargenInferior = 0
                }
            };

            mockDriverItc = new Mock<IDriverItc>();
            mockDriverItc.Setup(x => x.VerificarDispositivo()).Verifiable();
            mockDriverItc.Setup(x => x.ConsultarEstadoActual(It.IsAny<int>())).Returns(true);
            mockDriverItc.Setup(x => x.NotificarEstadoActual(It.IsAny<int>())).Verifiable();
        }

        [Test]
        public void TestInicializacion()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();

            Assert.DoesNotThrow(() => driver.Inicializar(codigoSensor, configSensor));
            Assert.That(driver.TipoDispositivo, Is.EqualTo(typeof(ConfigSensor)));
        }

        [Test]
        public void TestEventosSoportados()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);

            var eventosSoportados = new List<string>(driver.EventosSoportados);

            Assert.That(eventosSoportados, Has.Count.EqualTo(1));
            Assert.That(eventosSoportados, Contains.Item(CodigosEventos.EntradaActivada));
        }

        [Test]
        public void TestConsultaEstadoActual()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            var resultado = driver.ConsultaEstadoActual();

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.CodigoDispositivoSensor, Is.EqualTo(codigoSensor));
            Assert.That(resultado.EstadoActivo, Is.True);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            mockDriverItc.Verify(x => x.ConsultarEstadoActual(3), Times.Once());
        }

        [Test]
        public void TestNotificarEstadoActualSensor()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            Assert.DoesNotThrow(() => driver.NotificarEstadoActualSensor());
            mockDriverItc.Verify(x => x.NotificarEstadoActual(3), Times.Once());
        }

        [Test]
        public void TestVerificarDispositivo()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            Assert.DoesNotThrow(() => driver.VerificarDispositivo());
            mockDriverItc.Verify(x => x.VerificarDispositivo(), Times.Once());
        }

        [Test]
        public void TestEventoVehiculoDetectado_EntradaCorrecta()
        {
            var vehiculosDetectados = new List<NotificacionEvento>();
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            driver.EventoDriver += (sender, args) =>
            {
                lock (vehiculosDetectados)
                {
                    if (args.Notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                    {
                        vehiculosDetectados.Add(args.Notificacion);
                    }
                }
            };

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "3" },
                    { "Dato", "True" },
                    { "Mensaje", "True" }
                }
            };

            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacion });

            Thread.Sleep(100);

            lock (vehiculosDetectados)
            {
                Assert.That(vehiculosDetectados, Has.Count.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public void TestEventoVehiculoDetectado_EntradaIncorrecta_NoDispara()
        {
            var vehiculosDetectados = new List<NotificacionEvento>();
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            driver.EventoDriver += (sender, args) =>
            {
                lock (vehiculosDetectados)
                {
                    if (args.Notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                    {
                        vehiculosDetectados.Add(args.Notificacion);
                    }
                }
            };

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "5" },
                    { "Dato", "True" },
                    { "Mensaje", "True" }
                }
            };

            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacion });

            Thread.Sleep(100);

            lock (vehiculosDetectados)
            {
                Assert.That(vehiculosDetectados, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestEventoVehiculoDetectado_EntradaDesactivada_NoDispara()
        {
            var vehiculosDetectados = new List<NotificacionEvento>();
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            driver.EventoDriver += (sender, args) =>
            {
                lock (vehiculosDetectados)
                {
                    if (args.Notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                    {
                        vehiculosDetectados.Add(args.Notificacion);
                    }
                }
            };

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaDesactivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "3" },
                    { "Dato", "False" },
                    { "Mensaje", "False" }
                }
            };

            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacion });

            Thread.Sleep(100);

            lock (vehiculosDetectados)
            {
                Assert.That(vehiculosDetectados, Has.Count.EqualTo(0));
            }
        }

        [Test]
        public void TestEventoVehiculoDetectado_ContienePatente()
        {
            var vehiculosDetectados = new List<NotificacionEvento>();
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            driver.EventoDriver += (sender, args) =>
            {
                lock (vehiculosDetectados)
                {
                    if (args.Notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                    {
                        vehiculosDetectados.Add(args.Notificacion);
                    }
                }
            };

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "3" },
                    { "Dato", "True" },
                    { "Mensaje", "True" }
                }
            };

            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacion });

            Thread.Sleep(200);

            lock (vehiculosDetectados)
            {
                Assert.That(vehiculosDetectados, Has.Count.GreaterThanOrEqualTo(1));
                var evento = vehiculosDetectados[0];
                Assert.That(evento.CodigoDispositivo, Is.EqualTo(codigoSensor));
                Assert.That(evento.CodigoEvento, Is.EqualTo(CodigosEventos.VehiculoDetectado));
                Assert.That(evento.Datos.ContainsKey("Patente"), Is.True);
                Assert.That(evento.Datos.ContainsKey("Error"), Is.True);
                Assert.That(evento.Datos["Entrada"], Is.EqualTo("3"));
            }
        }

        [Test]
        public void TestEsEventoParaDispositivo_ValidaEntradaCorrecta()
        {
            var driver = new DriverSensorVehicular();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoSensor, configSensor);
            driver.DriverFisico = mockDriverItc.Object;

            var eventosRecibidos = 0;

            driver.EventoDriver += (sender, args) =>
            {
                if (args.Notificacion.CodigoEvento == CodigosEventos.VehiculoDetectado)
                {
                    Interlocked.Increment(ref eventosRecibidos);
                }
            };

            var notificacionCorrecta = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "3" },
                    { "Dato", "True" }
                }
            };

            var notificacionIncorrecta = new NotificacionEvento
            {
                CodigoDispositivo = "ITC01",
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>
                {
                    { "Entrada", "7" },
                    { "Dato", "True" }
                }
            };

            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacionCorrecta });
            Thread.Sleep(100);
            mockDriverItc.Raise(m => m.EventoDriver += null, mockDriverItc.Object, new EventoDriverEventArgs { Notificacion = notificacionIncorrecta });
            Thread.Sleep(100);

            Assert.That(eventosRecibidos, Is.EqualTo(1));
        }
    }
}
