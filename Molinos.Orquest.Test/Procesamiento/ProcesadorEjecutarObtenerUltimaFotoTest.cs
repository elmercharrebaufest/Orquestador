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
    public class ProcesadorEjecutarObtenerUltimaFotoTest
    {
        private ProcesadorEjecutarObtenerUltimaFoto target;
        
        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarObtenerUltimaFoto(new NullLogger());
        }

        [Test]
        public void TestProcesarOk()
        {
            var comando = new EjecutarObtenerUltimaFoto { CodigoDispositivo = "PAN1" };
            var dispositivo = new Dispositivo
                {
                    Codigo = "PAN1",
                    Configuracion = new ConfigPantalla {ClaseDriver = "MiDriver"}
                };

            var driverMock = new Mock<IDriverPantalla>();
            driverMock.Setup(d=>d.ObtenerUltimaFoto()).Returns(new ResultadoTomarFoto { Valores=null, Mensaje = new Mensaje(Codigos.OK, "ok") });
            var resultadoEjecutar = (ResultadoTomarFoto) target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarObtenerUltimaFoto { CodigoDispositivo = "PAN1" };
            var dispositivo = new Dispositivo
            {
                Codigo = "PAN1",
                Configuracion = new ConfigPantalla { ClaseDriver = "MiDriver" }
            };
            var driverMock = new Mock<IDriverPantalla>();
            driverMock.Setup(d => d.ObtenerUltimaFoto()).Throws<FormatoRespuestaDriverException>();
            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarObtenerUltimaFoto { CodigoDispositivo = "PAN1" };
            var dispositivo = new Dispositivo
            {
                Codigo = "PAN1",
                Configuracion = new ConfigPantalla { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverPantalla>();
            driverMock.Setup(d => d.ObtenerUltimaFoto()).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));
        }
    }
}
