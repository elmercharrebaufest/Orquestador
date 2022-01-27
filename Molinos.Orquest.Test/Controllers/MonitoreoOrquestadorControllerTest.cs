using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using NUnit.Framework;
using Moq;
namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
        "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class MonitoreoOrquestadorControllerTest
    {
        private MonitoreoOrquestadorController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IServicioOrquestador> servicioMock;

        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            servicioMock = new Mock<IServicioOrquestador>();
            target = new MonitoreoOrquestadorController(repositorioFactoryMock.Object,servicioMock.Object,new NullLogger());

            servicioMock.Setup(s => s.ObtenerEstadoServiceOrquestador(It.IsAny<string>())).Returns("OK");
            ConfigurationManager.AppSettings["HostServiciosWeb"] = "serv1;serv2";
        }

        [Test]
        public void SetearVista()
        {
            var result = target.Index() as ViewResult;
            Assert.NotNull(result);
            Assert.That(((List<MonitoreoOrquestadorDto>)(result.ViewBag.Servers)).Select(s => s.Maquina).ToList(),Is.EquivalentTo(new List<string>{"serv1","serv2"}));
        }

        [Test]
        public void Iniciar()
        {
            var result = target.Iniciar("serv1") as JsonResult;

            Assert.NotNull(result);
            Assert.AreEqual(result.Data,"El servicio se ha iniciado");

        }

        [Test]
        public void Detener()
        {
            var result = target.Detener("serv1") as JsonResult;

            Assert.NotNull(result);
            Assert.AreEqual(result.Data, "El servicio se ha detenido");

        }

        //[Test]
        //public void EstadoActualServicio()
        //{

        //    var result = target.EstadoActualServicio("serv1");

        //    Assert.NotNull(result);
        //    Assert.AreEqual(result,"OK");
        //}
    }
}
