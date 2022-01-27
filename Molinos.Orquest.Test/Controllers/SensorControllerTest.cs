using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web;
using Molinos.Orquest.Web.Controllers;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class SensorControllerTest
    {
        private SensorController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private Mock<IServicioOrquestador> servicioMock;
        private List<ConfigSensor> sensores;
        private List<string> drivers;
        private ConfigSensor configSensor;
        private ConfigSensor configSensor2;
        private Dispositivo dispositivo;
        private Dispositivo dispositivoConConcentrador;
        private List<Dispositivo> dispositivos;
        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverCamara>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });

            drivers = new List<string>() { "Driver1", "Driver2" };
            driverFactoryMock.Setup(x => x.DriversDisponibles<IDriverSensor>()).Returns(drivers);
            target = new SensorController(repositorioFactoryMock.Object, driverFactoryMock.Object, servicioMock.Object, new NullLogger());

            dispositivos = new List<Dispositivo>()
            {
                new Dispositivo()
                {
                    Activo = true,
                    Descripcion = "Dispositivo1",
                    Id = 1,
                    Codigo = "Codigo1"
                }
            };

            dispositivo = new Dispositivo()
            {
                Activo = true,
                Codigo = "a2",
                ConcentradorId = 1
            };

            dispositivoConConcentrador = new Dispositivo()
            {
                Activo = true,
                Codigo = "a2",
                ConcentradorId = 0,
                Concentrador = dispositivo,
                EsConcentrador = true
            };

            configSensor = new ConfigSensor()
            {
                ClaseDriver = "a",
                EstadoActivado = true,
                Id = 1,
                NumeroEntrada = 2,
                Dispositivo = dispositivo
            };

            configSensor2 = new ConfigSensor()
            {
                ClaseDriver = "a1",
                EstadoActivado = true,
                Id = 2,
                NumeroEntrada = 3,
                Dispositivo = dispositivoConConcentrador
            };

            sensores = new List<ConfigSensor>()
            {
                configSensor
            };
        }

        [Test]
        public void TestIndex()
        {
            var listaPaginada = new ListaPaginada<ConfigSensor>(sensores, 1, 40, 1);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(), It.IsAny<Paginacion>())).Returns(listaPaginada);
            var result = target.Index("a", 1, "aa", DirOrden.Asc) as ViewResult;
            ListaPaginada<ConfigSensor> items = (ListaPaginada<ConfigSensor>)target.ViewBag.Items;
            
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(result.Model, Is.TypeOf<string>());
            Assert.That(result.Model, Is.EqualTo("a"));
            Assert.That(items.Items[0].ClaseDriver, Is.EqualTo("a"));
            Assert.That(items.Items[0].EstadoActivado, Is.True);
            Assert.That(items.Items[0].Id, Is.EqualTo(1));
            Assert.That(items.Items[0].NumeroEntrada, Is.EqualTo(2));
            Assert.That(items.Items[0].Dispositivo.Activo, Is.True);
            Assert.That(items.Items[0].Dispositivo.Codigo, Is.EqualTo("a2"));
            Assert.That(items.Items[0].Dispositivo.ConcentradorId, Is.EqualTo(1));
        }

        [Test]
        public void TestListar()
        {
            var listaPaginada = new ListaPaginada<ConfigSensor>(sensores, 1, 40, 1);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(), It.IsAny<Paginacion>())).Returns(listaPaginada);
            var result = target.Listar("a", 1, "aa", DirOrden.Asc) as ViewResult;
            ListaPaginada<ConfigSensor> items = (ListaPaginada<ConfigSensor>)target.ViewBag.Items;

            Assert.That(result.Model, Is.Not.Null);
            Assert.That(result.Model, Is.TypeOf<string>());
            Assert.That(result.Model, Is.EqualTo("a"));
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(items.Items[0].ClaseDriver, Is.EqualTo("a"));
            Assert.That(items.Items[0].EstadoActivado, Is.True);
            Assert.That(items.Items[0].Id, Is.EqualTo(1));
            Assert.That(items.Items[0].NumeroEntrada, Is.EqualTo(2));
            Assert.That(items.Items[0].Dispositivo.Activo, Is.True);
            Assert.That(items.Items[0].Dispositivo.Codigo, Is.EqualTo("a2"));
            Assert.That(items.Items[0].Dispositivo.ConcentradorId, Is.EqualTo(1));

            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(), It.IsAny<Paginacion>()), Times.Once());
        }

        [Test]
        public void TestCrear()
        {
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                .Returns(dispositivos);
            
            var result = target.Crear() as ViewResult;
            var puedeConcentrador = (bool) result.ViewBag.PuedeSerConcentrador;
            var driversStrings = (List<SelectListItem>) result.ViewBag.Drivers;
            var concentradores = (List<SelectListItem>) result.ViewBag.Concentradores;

            Assert.That(concentradores[0].Text, Is.EqualTo("No Tiene"));
            Assert.That(concentradores[1].Text, Is.EqualTo("Dispositivo1"));
            Assert.That(driversStrings[0].Text, Is.EqualTo("Driver1"));
            Assert.That(puedeConcentrador, Is.False);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.Null.Or.Empty);

            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()), Times.Once());
            driverFactoryMock.Verify(x => x.DriversDisponibles<IDriverSensor>(), Times.Once());
        }

        [Test]
        public void TestCrearPost1()
        {
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(It.IsAny<int>())).Returns(dispositivo);
            var result = target.Crear(configSensor) as AjaxEditSuccessResult;

            Assert.That(result, Is.TypeOf<AjaxEditSuccessResult>());
            Assert.That(result.Content, Is.EqualTo("ajax-edit-success"));
            repositorioMock.Verify(x => x.Obtener<Dispositivo>(It.IsAny<int>()), Times.Exactly(2));
            repositorioMock.Verify(x => x.Agregar(configSensor), Times.Once());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearPost2()
        {
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(It.IsAny<int>())).Returns(dispositivoConConcentrador);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(dispositivos);
            var result = target.Crear(configSensor2) as AjaxEditSuccessResult;

            Assert.That(result, Is.TypeOf<AjaxEditSuccessResult>());
            Assert.That(result.Content, Is.EqualTo("ajax-edit-success"));
            repositorioMock.Verify(x => x.Obtener<Dispositivo>(It.IsAny<int>()), Times.Never());
            repositorioMock.Verify(x => x.Agregar(configSensor2), Times.Once());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearPost4()
        {
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(It.IsAny<int>())).Returns(dispositivo);
            repositorioMock.Setup(x => x.Existe(It.IsAny<Expression<Func<ConfigSensor, bool>>>())).Returns(false);
            repositorioMock.Setup(x => x.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(true);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(dispositivos);

            var result = target.Crear(configSensor) as ViewResult;

            var puedeConcentrador = (bool)result.ViewBag.PuedeSerConcentrador;
            var driversStrings = (List<SelectListItem>)result.ViewBag.Drivers;
            var concentradores = (List<SelectListItem>)result.ViewBag.Concentradores;

            Assert.That(driversStrings[0].Text, Is.EqualTo("Driver1"));
            Assert.That(puedeConcentrador, Is.False);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
            Assert.That(result.Model, Is.TypeOf<ConfigSensor>());
            Assert.That(concentradores[0].Text, Is.EqualTo("No Tiene"));
            Assert.That(concentradores[1].Text, Is.EqualTo("Dispositivo1"));

            repositorioMock.Verify(x => x.Obtener<Dispositivo>(It.IsAny<int>()), Times.Once());
            repositorioMock.Verify(x => x.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()), Times.Once());
            repositorioMock.Verify(x => x.Existe(It.IsAny<Expression<Func<Suscripcion, bool>>>()), Times.Never());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()), Times.Once());
        }

        [Test]
        public void TestModificar()
        {
            repositorioMock.Setup(x => x.Obtener<ConfigSensor>(It.IsAny<int>())).Returns(configSensor);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(dispositivos);

            var result = target.Modificar(1) as ViewResult;

            var puedeConcentrador = (bool)result.ViewBag.PuedeSerConcentrador;
            var driversStrings = (List<SelectListItem>)result.ViewBag.Drivers;
            var concentradores = (List<SelectListItem>)result.ViewBag.Concentradores;

            Assert.That(driversStrings[0].Text, Is.EqualTo("Driver1"));
            Assert.That(puedeConcentrador, Is.False);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(true));
            Assert.That(result.Model, Is.TypeOf<ConfigSensor>());
            Assert.That(concentradores[0].Text, Is.EqualTo("No Tiene"));
            Assert.That(concentradores[1].Text, Is.EqualTo("Dispositivo1"));
        }

        [Test]
        public void TestModificarPostModelInvalid()
        {
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(dispositivos);
            target.ModelState.AddModelError("Error", "Error");
            var result = target.Modificar(configSensor) as ViewResult;

            var puedeConcentrador = (bool)result.ViewBag.PuedeSerConcentrador;
            var driversStrings = (List<SelectListItem>)result.ViewBag.Drivers;
            var concentradores = (List<SelectListItem>)result.ViewBag.Concentradores;

            Assert.That(driversStrings[0].Text, Is.EqualTo("Driver1"));
            Assert.That(puedeConcentrador, Is.False);
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
            Assert.That(result.Model, Is.TypeOf<ConfigSensor>());
            Assert.That(concentradores[0].Text, Is.EqualTo("No Tiene"));
            Assert.That(concentradores[1].Text, Is.EqualTo("Dispositivo1"));
        }

        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(dispositivos);
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(It.IsAny<int>())).Returns(dispositivo);
            configSensor.Dispositivo.Configuracion = new ConfigSensor()
            {
                ClaseDriver = "a"
            };
            var result = target.Modificar(configSensor) as AjaxEditSuccessResult;

            Assert.That(result, Is.TypeOf<AjaxEditSuccessResult>());
            Assert.That(result.Content, Is.EqualTo("ajax-edit-success"));
            repositorioMock.Verify(x => x.Obtener<Dispositivo>(It.IsAny<int>()), Times.Exactly(2));
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestEliminar()
        {
            repositorioMock.Setup(x => x.Obtener<ConfigSensor>(It.IsAny<int>())).Returns(configSensor);

            var request = new Mock<HttpRequestBase>();
            request.Expect(r => r.HttpMethod).Returns("GET");
            var mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Expect(c => c.Request).Returns(request.Object);
            target.ControllerContext = new ControllerContext(mockHttpContext.Object, new RouteData(), new Mock<ControllerBase>().Object);

            var result = target.Eliminar(1) as RedirectToRouteResult;

            Assert.That(result.RouteValues.ContainsValue("Index"), Is.EqualTo(true));
            repositorioMock.Verify(x => x.Obtener<ConfigSensor>(It.IsAny<int>()), Times.Exactly(1));
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());
            repositorioMock.Verify(x => x.Remover(configSensor.Dispositivo), Times.Once());
            repositorioMock.Verify(x => x.Remover(configSensor), Times.Once());
        }
    }
}
