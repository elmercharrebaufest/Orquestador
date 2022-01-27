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
    public class ProcesadorEjecutarPesajeTest
    {
        private ProcesadorEjecutarPesaje target;
        
        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarPesaje(new NullLogger());
        }

        [Test]
        public void TestProcesarOK()
        {
            var comando = new EjecutarPesaje { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
                {
                    Codigo = "BAL01",
                    Configuracion = new ConfigCabezal {ClaseDriver = "MiDriver"}
                };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.ObtenerPeso()).Returns(123);

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            Assert.That(resultadoEjecutar.Valores, Is.Not.Null.And.Count.EqualTo(1));
            Assert.That(resultadoEjecutar.Valores["Pesaje"], Is.EqualTo(123));
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarPesaje { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.ObtenerPeso()).Throws<FormatoRespuestaDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarPesaje { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.ObtenerPeso()).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarPesaje { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.ObtenerPeso()).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarSinPesoEstable()
        {
            var comando = new EjecutarPesaje { CodigoDispositivo = "BAL01" };
            var dispositivo = new Dispositivo
            {
                Codigo = "BAL01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCabezal>();
            driverMock.Setup(d => d.ObtenerPeso()).Returns((decimal?) null);

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.CabezalPesoNoEstable));
            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }
    }
}
