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
    class ProcesadorComandoDepurarSuscripcionesTest
    {
        private ProcesadorComandoDepurarSuscripciones target;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock;
        private Mock<IDriver> driverMock;

        [SetUp]
        public void SetUp()
        {
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();
            target = new ProcesadorComandoDepurarSuscripciones(adminSuscripcionesMock.Object, new NullLogger());

            driverMock = new Mock<IDriver>();
        }

        [Test]
        public void TestProcesarOK()
        {
            var comando = new ComandoDepurarSuscripciones
            {
                CodigoDispositivo = "HUM01",
                Vencimiento = new DateTime(2013, 10, 25)
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            adminSuscripcionesMock.Verify(adm => adm.DepurarSuscripciones(123, new DateTime(2013, 10, 25), false), Times.Once());
        }
    }
}
