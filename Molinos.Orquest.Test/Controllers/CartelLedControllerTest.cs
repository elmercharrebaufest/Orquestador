using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class CartelLedControllerTest
    {
        private CartelLedController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IServicioOrquestador> servicioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private Mock<ILogger> log;
        private IConversor conversor;
        private List<ConfigCartelLed> cartelesLeds;
        private List<Dispositivo> dispositivos;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverCartelLed>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            conversor = FactoryConversor.ConversorAutoMapper;
            log = new Mock<ILogger>();
            target = new CartelLedController(repositorioFactoryMock.Object, driverFactoryMock.Object, conversor, servicioMock.Object, log.Object);
            cartelesLeds = new List<ConfigCartelLed>
            {
                new ConfigCartelLed
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        ControlBrillo = 1,
                        DireccionIp = "host",
                        Efecto = 1,
                        LongFrase = 5,
                        Tipografia = 1,
                        VelocidadScroll = 3,
                        Puerto = 10,
                        TimeoutLectura = 10,
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigCartelLed{Dispositivo = new Dispositivo()}
            };

            dispositivos = new List<Dispositivo>
                {
                    new Dispositivo{Codigo = "1", Descripcion = "11", EsConcentrador = true, Id = 1},
                    new Dispositivo{Codigo = "2", Descripcion = "22", Id = 2}
                };
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCartelLed, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCartelLed>(cartelesLeds, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigCartelLedModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCartelLed, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCartelLed>(cartelesLeds, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigCartelLedModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }
        [Test]
        public void TestCrear()
        {
            var result = target.Crear() as ViewResult;
            var drivers = (List<SelectListItem>)result.ViewBag.Drivers;
            var dispositivos = (List<SelectListItem>)result.ViewBag.Concentradores;
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(drivers.Count, Is.EqualTo(2));
            Assert.That(dispositivos.Count, Is.EqualTo(2));
        }
        [Test]
        public void TestCrearPost()
        {
            var cartelLed = new ConfigCartelLedModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                LongFrase = 5,
                ControlBrillo = 1,
                VelocidadScroll = 1,
                Tipografia = 1,
                DireccionIp = "1011",
                Efecto = 1,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };

            var result = target.Crear(cartelLed) as ActionResult;
            Assert.NotNull(result);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCartelLed>(f => f.Dispositivo.Codigo == cartelLed.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());

        }

        [Test]
        public void TestModificar()
        {
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigCartelLed>(It.IsAny<int>())).Returns(cartelesLeds[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestProbar()
        {

            repositorioMock.Setup(s => s.Obtener<ConfigCartelLed>(1)).Returns(cartelesLeds[0]);
            var result = target.Probar(1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as ConfigCartelLedModel;
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(cartelesLeds[0].Id));
        }
    }
}
