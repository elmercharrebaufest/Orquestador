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
    class ProcesadorEjecutarBorrarBalanzadaTest
    {
        private ProcesadorEjecutarBorrarBalanzada target;
        private Mock<IDriverBalanzaPuerto> driverMock;

        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarBorrarBalanzada(new NullLogger());
            driverMock = new Mock<IDriverBalanzaPuerto>();
        }

        [Test]
        public void TestConsultaUltimaBalanzada()
        {
            var comando = new EjecutarBorrarBalanzada
            {
                    CodigoDispositivo = "321",
                    IdBorrado = 123
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.SetupGet(d => d.EventosSoportados).Returns(new[] {CodigosEventos.BalanzadaRecibida});
                        
            var resultado = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);            
        }
                
        [Test]
        public void TestErrorEnDriverConexion()
        {
            var comando = new EjecutarBorrarBalanzada
            {
                CodigoDispositivo = "321",
                IdBorrado = 10000000
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.Setup(d => d.BorrarBalanzada(comando.IdBorrado)).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarBorrarBalanzada
            {
                CodigoDispositivo = "321",
                IdBorrado = 10000000
            };

            var dispositivo = new Dispositivo
            {
                Id = 123,
                Codigo = "321",
                Configuracion = new ConfigBalanzaPuerto { ClaseDriver = "MiDriver" }
            };

            driverMock.Setup(d => d.ConsultaBalanzada(comando.IdBorrado)).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

    }
}
