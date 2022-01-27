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
    class ProcesadorEjecutarConsultaBalanzadaPorRangoTest
    {
        private ProcesadorEjecutarConsultaBalanzadaPorRango target;
        private Mock<IDriverBalanzaPuerto> driverMock;

        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarConsultaBalanzadaPorRango(new NullLogger());
            driverMock = new Mock<IDriverBalanzaPuerto>();
        }

        [Test]
        public void TestConsultaBalanzadaPorRango()
        {
            var comando = new EjecutarConsultaBalanzadaPorRango
            {
                CodigoDispositivo = "321",
                IdBalanzadaInicio = 10000000, //un valor que espero nunca tenga balanzada
                IdBalanzadaFin = 10000002
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] { CodigosEventos.BalanzadaRecibida });

            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.BalanzadaNoEncontrada));
        }

        [Test]
        public void TestErrorEnDriverConexion()
        {
            var comando = new EjecutarConsultaBalanzadaPorRango
            {
                CodigoDispositivo = "321",
                IdBalanzadaInicio = 10000000, 
                IdBalanzadaFin = 10000002
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.Setup(d => d.ConsultaBalanzada(comando.IdBalanzadaInicio)).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarConsultaBalanzadaPorRango
            {
                CodigoDispositivo = "321",
                IdBalanzadaInicio = 10000000,
                IdBalanzadaFin = 10000002
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.Setup(d => d.ConsultaBalanzada(comando.IdBalanzadaFin)).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }
    }
}
