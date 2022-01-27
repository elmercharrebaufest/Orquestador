using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Procesamiento
{
    [TestFixture]
    class ProcesadorComandoSuscribirTest
    {
        private ProcesadorComandoSuscribir target;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock;
        private Mock<IDriver> driverMock;

        [SetUp]
        public void SetUp()
        {
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();
            target = new ProcesadorComandoSuscribir(adminSuscripcionesMock.Object, new NullLogger());

            driverMock = new Mock<IDriver>();
        }

        [Test]
        public void TestProcesarOKSinSuscripcionesPreviasActivas()
        {
            var comando = new ComandoSuscribir
                {
                    CodigoDispositivo = "HUM01",
                    CodigoEvento = CodigosEventos.HumedadRecibida,
                    RutaAccesoSuscriptor = "http://unaurl"
                };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] {CodigosEventos.HumedadRecibida});

            adminSuscripcionesMock
                .Setup(adm => adm.CrearSuscripcion(123, CodigosEventos.HumedadRecibida, "http://unaurl", false))
                .Returns(456);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(456));

            adminSuscripcionesMock
                .Verify(adm => adm.CrearSuscripcion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void TestProcesarOKConSuscripcionesPreviasActivas()
        {
            var comando = new ComandoSuscribir
            {
                CodigoDispositivo = "HUM01",
                CodigoEvento = CodigosEventos.HumedadRecibida,
                RutaAccesoSuscriptor = "http://unaurl"
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.HumedadRecibida });
            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(123)).Returns(true);
            adminSuscripcionesMock
                .Setup(adm => adm.CrearSuscripcion(123, CodigosEventos.HumedadRecibida, "http://unaurl", false))
                .Returns(456);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(456));

            adminSuscripcionesMock
                .Verify(adm => adm.CrearSuscripcion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());

        }

        [Test]
        public void TestProcesarErrorDriverNoSoportaEvento()
        {
            var comando = new ComandoSuscribir
            {
                CodigoDispositivo = "HUM01",
                CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                RutaAccesoSuscriptor = "http://unaurl"
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.HumedadRecibida });

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.EventoNoSoportado));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(0));

            adminSuscripcionesMock
                .Verify(adm => adm.CrearSuscripcion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new ComandoSuscribir
            {
                CodigoDispositivo = "HUM01",
                CodigoEvento = CodigosEventos.HumedadRecibida,
                RutaAccesoSuscriptor = "http://unaurl"
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.HumedadRecibida });

            adminSuscripcionesMock
                .Setup(adm => adm.CrearSuscripcion(123, CodigosEventos.HumedadRecibida, "http://unaurl", false))
                .Returns(456);

            driverMock.SetupGet(d => d.EventosSoportados).Throws<DriverException>();

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(0));

            adminSuscripcionesMock
                .Verify(adm => adm.CrearSuscripcion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void TestProcesarErrorDesconocido()
        {
            var comando = new ComandoSuscribir
            {
                CodigoDispositivo = "HUM01",
                CodigoEvento = CodigosEventos.HumedadRecibida,
                RutaAccesoSuscriptor = "http://unaurl"
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.HumedadRecibida });

            adminSuscripcionesMock
                .Setup(adm => adm.CrearSuscripcion(123, CodigosEventos.HumedadRecibida, "http://unaurl", false))
                .Throws<Exception>();

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.Error));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(0));
        }

        [Test]
        public void TestProcesarOKConSuscripcionesPreviasActivas2()
        {
            var comando = new ComandoSuscribir
            {
                CodigoDispositivo = "HUM01",
                CodigoEvento = CodigosEventos.ConexionDispositivoCorrecta,
                RutaAccesoSuscriptor = "http://unaurl"
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.HumedadRecibida, CodigosEventos.ConexionDispositivoCorrecta, CodigosEventos.ErrorConexionDispositivo });
            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(123)).Returns(true);
            adminSuscripcionesMock
                .Setup(adm => adm.CrearSuscripcion(123, CodigosEventos.HumedadRecibida, "http://unaurl", false))
                .Returns(456);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(0));

            adminSuscripcionesMock
                .Verify(adm => adm.CrearSuscripcion(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Once());

        }
    }
}
