using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Procesamiento
{
    [TestFixture]
    public class ProcesadorEjecutarVerificacionDispositivoTest
    {
        private ProcesadorEjecutarVerificacionDispositivo target;

        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarVerificacionDispositivo(new NullLogger());
        }

        [Test]
        public void TestProcesarOK()
        {
            var comando = new EjecutarVerificacionDispositivo { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriver>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);

            driverMock.Verify(d => d.VerificarDispositivo(), Times.Once());
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarVerificacionDispositivo { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.VerificarDispositivo()).Throws<FormatoRespuestaDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarVerificacionDispositivo { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.VerificarDispositivo()).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarVerificacionDispositivo { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.VerificarDispositivo()).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

    }
}
