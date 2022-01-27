using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Web.Mvc;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Models;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class PruebaConexionControllerTest
    {
        private Mock<Ping> pingMock;
        private PruebaConexionController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IServicioOrquestador> servicioMock;
        private Mock<IDriverFactory> driverFactoryMock;

        [SetUp]
        public void SetUp()
        {
            pingMock = new Mock<Ping>();
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            driverFactoryMock = new Mock<IDriverFactory>();
            target = new PruebaConexionController(repositorioFactoryMock.Object, servicioMock.Object, new NullLogger(), driverFactoryMock.Object);
        }

        [Test]
        public void TestIndex()
        {
            var result = target.Index("192.168.23.12", 1470, "") as ViewResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(target.ViewBag.IpDispositivo, Is.EqualTo("192.168.23.12"));
            Assert.That(target.ViewBag.PuertoDispositivo, Is.EqualTo(1470));
            Assert.That(result.View, Is.Null);
        }

        [Test]
        public void TestPingTimeOut()
        {
            var result = target.Ping("192.168.23.12") as ViewResult;
            Assert.That(result, Is.Not.Null);
            //Assert.That(result.View, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("ResultadoPrueba"));
            Assert.That(result.Model, Is.TypeOf<List<ResultadoPruebaModel>>());
            var modelo = ((IList<ResultadoPruebaModel>) result.Model);

            Assert.That(modelo[0].Error, Is.True);
            Assert.That(modelo[1].Error, Is.True);
            Assert.That(modelo[2].Error, Is.True);
            Assert.That(modelo[3].Error, Is.True);

            Assert.That(modelo[0].Message, Is.EqualTo("El intento de conexión dio timeout..."));
            Assert.That(modelo[1].Message, Is.EqualTo("El intento de conexión dio timeout..."));
            Assert.That(modelo[2].Message, Is.EqualTo("El intento de conexión dio timeout..."));
            Assert.That(modelo[3].Message, Is.EqualTo("El intento de conexión dio timeout..."));
        }

        [Test]
        public void TestPingSuccess()
        {
            var result = target.Ping("127.0.0.1") as ViewResult;
            Assert.That(result, Is.Not.Null);
            //Assert.That(result.View, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("ResultadoPrueba"));
            Assert.That(result.Model, Is.TypeOf<List<ResultadoPruebaModel>>());
            var modelo = ((IList<ResultadoPruebaModel>)result.Model);

            Assert.That(modelo[0].Error, Is.False);
            Assert.That(modelo[1].Error, Is.False);
            Assert.That(modelo[2].Error, Is.False);
            Assert.That(modelo[3].Error, Is.False);

            Assert.That(modelo[0].Message, Is.EqualTo("Respuesta de 127.0.0.1: bytes=32 tiempo=0ms TTL=128"));
            Assert.That(modelo[1].Message, Is.EqualTo("Respuesta de 127.0.0.1: bytes=32 tiempo=0ms TTL=128"));
            Assert.That(modelo[2].Message, Is.EqualTo("Respuesta de 127.0.0.1: bytes=32 tiempo=0ms TTL=128"));
            Assert.That(modelo[3].Message, Is.EqualTo("Respuesta de 127.0.0.1: bytes=32 tiempo=0ms TTL=128"));
        }

        [Test]
        public void TestPingFallido()
        {
            var result = target.Ping("127") as ViewResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("ResultadoPrueba"));
            Assert.That(result.Model, Is.TypeOf<List<ResultadoPruebaModel>>());
            var modelo = ((IList<ResultadoPruebaModel>)result.Model);

            Assert.That(modelo[0].Error, Is.True);
            Assert.That(modelo[1].Error, Is.True);
            Assert.That(modelo[2].Error, Is.True);
            Assert.That(modelo[3].Error, Is.True);

            Assert.That(modelo[0].Message, Is.EqualTo("Error de conexión: An exception occurred during a Ping request."));
            Assert.That(modelo[1].Message, Is.EqualTo("Error de conexión: An exception occurred during a Ping request."));
            Assert.That(modelo[2].Message, Is.EqualTo("Error de conexión: An exception occurred during a Ping request."));
            Assert.That(modelo[3].Message, Is.EqualTo("Error de conexión: An exception occurred during a Ping request."));
        }

        [Test]
        public void TestLoopback()
        {
            var result = target.Loopback("127.0.0.1", 80, 1) as ViewResult;

            Assert.That(result.ViewName, Is.EqualTo("ResultadoPrueba"));
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(((List<ResultadoPruebaModel>)result.Model)[0].Message, Is.EqualTo("Prueba fallida"));
        }
    }
}
