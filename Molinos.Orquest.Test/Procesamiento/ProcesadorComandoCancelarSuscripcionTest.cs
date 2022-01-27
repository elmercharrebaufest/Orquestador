using System;
using Molinos.Orquest.Dominio;
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
    class ProcesadorComandoCancelarSuscripcionTest
    {
        private ProcesadorComandoCancelarSuscripcion target;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock;
        private Mock<IDriver> driverMock;

        [SetUp]
        public void SetUp()
        {
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();
            target = new ProcesadorComandoCancelarSuscripcion(adminSuscripcionesMock.Object, new NullLogger());

            driverMock = new Mock<IDriver>();
        }

        [Test]
        public void TestProcesarOKCancelaYNoQuedanOtrasOtrasActivas()
        {
            var comando = new ComandoCancelarSuscripcion
            {
                CodigoDispositivo = "HUM01",
                IdSuscripcion = 456
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(123)).Returns(false);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(456, 123, false), Times.Once());
            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void TestProcesarOKCancelaTodasParaUnSuscriptorYNoQuedanOtrasOtrasActivas()
        {
            var comando = new ComandoCancelarSuscripcion
            {
                CodigoDispositivo = "HUM01",
                IdSuscripcion = 456,
                CancelarTodas = true
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(123)).Returns(false);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(456, 123, true), Times.Once());
            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void TestProcesarOKCancelaYQuedanOtrasOtrasActivas()
        {
            var comando = new ComandoCancelarSuscripcion
            {
                CodigoDispositivo = "HUM01",
                IdSuscripcion = 456
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(123)).Returns(true);

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(456, 123, false), Times.Once());
            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void TestProcesarErrorCancelaSuscripcionInexistente()
        {
            var comando = new ComandoCancelarSuscripcion
            {
                CodigoDispositivo = "HUM01",
                IdSuscripcion = 999
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            adminSuscripcionesMock.Setup(adm => adm.CancelarSuscripcion(999, 123, false)).Throws<SuscripcionNoEncontradaException>();

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.SuscripcionInexistente));

            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(999, 123, false), Times.Once());
            adminSuscripcionesMock.Verify(adm => adm.CancelarSuscripcion(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void TestProcesarErrorDesconocido()
        {
            var comando = new ComandoCancelarSuscripcion
            {
                CodigoDispositivo = "HUM01",
                IdSuscripcion = 456
            };

            var dispositivo = new Dispositivo
                {
                    Id = 123,
                    Codigo = "HUM01",
                    Configuracion = new ConfigHumedimetro {ClaseDriver = "MiDriver"}
                };

            adminSuscripcionesMock
                .Setup(adm => adm.CancelarSuscripcion(456, 123, false))
                .Throws<Exception>();

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.Error));
        }
    }
}
