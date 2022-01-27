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
    public class ProcesadorEjecutarTomarFotoTest
    {
        private ProcesadorEjecutarTomarFoto target;
        
        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarTomarFoto(new NullLogger());
        }

        [Test]
        public void TestProcesarOk()
        {
            var comando = new EjecutarTomarFoto { CodigoDispositivo = "CAM1" };
            var dispositivo = new Dispositivo
                {
                    Codigo = "CAM1",
                    Configuracion = new ConfigCamara {ClaseDriver = "MiDriver"}
                };

            var driverMock = new Mock<IDriverCamara>();
            driverMock.Setup(d=>d.TomarFoto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new ResultadoTomarFoto { Valores=null, Mensaje = new Mensaje(Codigos.OK, "ok") });
            var resultadoEjecutar = (ResultadoTomarFoto) target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarTomarFoto { CodigoDispositivo = "CAM1" };
            var dispositivo = new Dispositivo
            {
                Codigo = "CAM1",
                Configuracion = new ConfigCamara { ClaseDriver = "MiDriver" }
            };
            var driverMock = new Mock<IDriverCamara>();
            driverMock.Setup(d => d.TomarFoto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<FormatoRespuestaDriverException>();
            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarTomarFoto { CodigoDispositivo = "CAM1" };
            var dispositivo = new Dispositivo
            {
                Codigo = "CAM1",
                Configuracion = new ConfigCamara { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverCamara>();
            driverMock.Setup(d => d.TomarFoto(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));
        }
    }
}
