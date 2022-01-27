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
    public class ProcesadorEjecutarAperturaBarreraTest
    {
        private ProcesadorEjecutarAperturaBarrera target;

        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarAperturaBarrera(new NullLogger());
        }

        [Test]
        public void TestProcesarOK()
        {
            var comando = new EjecutarAperturaBarrera { CodigoDispositivo = "BAR01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAR01",
                Configuracion = new ConfigBarrera { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverBarrera>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarAperturaBarrera { CodigoDispositivo = "BAR01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAR01",
                Configuracion = new ConfigBarrera { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverBarrera>();
            driverMock.Setup(d => d.Abrir()).Throws<FormatoRespuestaDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarAperturaBarrera { CodigoDispositivo = "BAR01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAR01",
                Configuracion = new ConfigBarrera { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverBarrera>();
            driverMock.Setup(d => d.Abrir()).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarAperturaBarrera { CodigoDispositivo = "BAR01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAR01",
                Configuracion = new ConfigBarrera { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverBarrera>();
            driverMock.Setup(d => d.Abrir()).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }
    }
}
