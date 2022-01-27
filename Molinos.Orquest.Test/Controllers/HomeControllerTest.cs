using System.Web.Mvc;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Controllers;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class HomeControllerTest
    {
        private HomeController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IServicioOrquestador> servicioMock;
        private Mock<ILogger> log;
        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            log = new Mock<ILogger>();
            target = new HomeController(repositorioFactoryMock.Object, servicioMock.Object, log.Object);
        }

        [Test]
        public void TestIndex()
        {
            var result = target.Index() as ViewResult;

            Assert.That(result.Model, Is.Null);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
        }
    }
}
