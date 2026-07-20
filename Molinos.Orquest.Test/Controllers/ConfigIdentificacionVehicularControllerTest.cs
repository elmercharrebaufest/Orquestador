using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class ConfigIdentificacionVehicularControllerTest
    {
        private ConfigIdentificacionVehicularController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IConversor> conversorMock;
        private Mock<IServicioOrquestador> servicioMock;

        private List<ConfigIdentificacionVehicular> configs;
        private List<ConfigLectorTarjetas> lectores;
        private List<ConfigSensor> sensores;
        private List<ConfigCamara> camaras;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            conversorMock = new Mock<IConversor>();
            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

            target = new ConfigIdentificacionVehicularController(
                repositorioFactoryMock.Object, conversorMock.Object, servicioMock.Object, new NullLogger());

            var dispositivoLector  = new Dispositivo { Id = 20, Codigo = "LEC-01", Descripcion = "Lector 01",  Activo = true };
            var dispositivoSensor  = new Dispositivo { Id = 30, Codigo = "SEN-01", Descripcion = "Sensor 01",  Activo = true };
            var dispositivoCamara  = new Dispositivo { Id = 40, Codigo = "CAM-01", Descripcion = "Camara 01",  Activo = true };

            lectores = new List<ConfigLectorTarjetas>
            {
                new ConfigLectorTarjetas { Id = 20, Lector = "L1", Dispositivo = dispositivoLector }
            };

            sensores = new List<ConfigSensor>
            {
                new ConfigSensor { Id = 30, NumeroEntrada = 1, Dispositivo = dispositivoSensor }
            };

            camaras = new List<ConfigCamara>
            {
                new ConfigCamara { Id = 40, Uri = "http://cam", TimeoutLectura = 5000, DireccionIp = "192.168.1.100", Dispositivo = dispositivoCamara }
            };

            configs = new List<ConfigIdentificacionVehicular>
            {
                new ConfigIdentificacionVehicular
                {
                    Id = 1,
                    Nombre = "Config Norte",
                    Codigo = "CFG-001",
                    ConfigLectorTarjetasId = 20,
                    Activo = true,
                    Camaras = new List<ConfigIdentificacionVehicularCamara>()
                }
            };

            // Stub generic Listar<T>() used by PopularLectoresYSensores
            repositorioMock.Setup(r => r.Listar<ConfigLectorTarjetas>(null))
                .Returns(lectores);
            repositorioMock.Setup(r => r.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(sensores);
            repositorioMock.Setup(r => r.Listar<ConfigCamara>(null))
                .Returns(camaras);
        }

        private ControllerContext BuildControllerContext(bool isAjax = false)
        {
            var headers = new System.Net.WebHeaderCollection();
            if (isAjax)
                headers["X-Requested-With"] = "XMLHttpRequest";

            var server  = new Mock<HttpServerUtilityBase>();
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(r => r.Headers).Returns(headers);
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            context.SetupGet(x => x.Request).Returns(request.Object);
            return new ControllerContext(context.Object, new RouteData(), target);
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(r => r.Listar(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>(),
                    It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<ConfigIdentificacionVehicular>(configs, 1, 8, 1));

            var result = target.Index(string.Empty) as ViewResult;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That((object)target.ViewBag.Items, Is.Not.Null);
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(r => r.Listar(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>(),
                    It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<ConfigIdentificacionVehicular>(configs, 1, 8, 1));

            var result = target.Listar(string.Empty) as ViewResult;

            Assert.That(result.ViewName, Is.EqualTo("Listar"));
        }

        [Test]
        public void TestCrear_Get()
        {
            var result = target.Crear() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewBag.Lectores, Is.Not.Null);
            Assert.That(result.ViewBag.SensoresVehiculares, Is.Not.Null);
        }

        [Test]
        public void TestCrear_Post_Valido()
        {
            repositorioMock.Setup(r => r.Existe(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(false);

            var model = new ConfigIdentificacionVehicularModel
            {
                Nombre = "Config Sur",
                Codigo = "CFG-002",
                ConfigLectorTarjetasId = 20,
                MaxReintentosFoto = 3,
                DelayEntreReintentosMs = 500,
                Activo = true
            };

            target.ControllerContext = BuildControllerContext();
            var result = target.Crear(model, new[] { 40 }) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
            repositorioMock.Verify(r => r.Agregar(It.Is<ConfigIdentificacionVehicular>(c => c.Codigo == "CFG-002")), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrear_Post_SinDispositivoDisparador_Invalido()
        {
            repositorioMock.Setup(r => r.Existe(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(false);

            var model = new ConfigIdentificacionVehicularModel
            {
                Nombre = "Config Sin Dispositivo",
                Codigo = "CFG-003",
                ConfigLectorTarjetasId = null,
                ConfigSensorVehicularId = null,
                MaxReintentosFoto = 3,
                DelayEntreReintentosMs = 500,
                Activo = true
            };

            target.ControllerContext = BuildControllerContext();
            var result = target.Crear(model, null) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(target.ModelState.IsValid, Is.False);
            repositorioMock.Verify(r => r.Agregar(It.IsAny<ConfigIdentificacionVehicular>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Never());
        }

        [Test]
        public void TestCrear_Post_CodigoDuplicado_Invalido()
        {
            repositorioMock.Setup(r => r.Existe(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns<Expression<Func<ConfigIdentificacionVehicular, bool>>>(q => configs.Any(q.Compile()));

            var model = new ConfigIdentificacionVehicularModel
            {
                Nombre = "Config Norte Copia",
                Codigo = "CFG-001",  // duplicado
                ConfigLectorTarjetasId = 20,
                MaxReintentosFoto = 3,
                DelayEntreReintentosMs = 500,
                Activo = true
            };

            target.ControllerContext = BuildControllerContext();
            var result = target.Crear(model, null) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(target.ModelState.IsValid, Is.False);
            repositorioMock.Verify(r => r.Agregar(It.IsAny<ConfigIdentificacionVehicular>()), Times.Never());
        }

        [Test]
        public void TestModificar_Get()
        {
            var model = new ConfigIdentificacionVehicularModel { Id = 1, Nombre = "Config Norte", Codigo = "CFG-001", ConfigLectorTarjetasId = 20, Activo = true };
            repositorioMock.Setup(r => r.Obtener<ConfigIdentificacionVehicular>(1)).Returns(configs[0]);
            conversorMock.Setup(x => x.Convertir<ConfigIdentificacionVehicular, ConfigIdentificacionVehicularModel>(It.IsAny<ConfigIdentificacionVehicular>())).Returns(model);

            target.ControllerContext = BuildControllerContext();
            var result = target.Modificar(1) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewBag.SensoresVehiculares, Is.Not.Null);
            conversorMock.Verify(x => x.Convertir<ConfigIdentificacionVehicular, ConfigIdentificacionVehicularModel>(It.IsAny<ConfigIdentificacionVehicular>()), Times.Once());
        }

        [Test]
        public void TestModificar_Post_Valido()
        {
            repositorioMock.Setup(r => r.Existe(
                    It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(false);
            repositorioMock.Setup(r => r.Obtener<ConfigIdentificacionVehicular>(1)).Returns(configs[0]);

            var model = new ConfigIdentificacionVehicularModel
            {
                Id = 1,
                Nombre = "Config Norte Actualizada",
                Codigo = "CFG-001",
                ConfigLectorTarjetasId = 20,
                MaxReintentosFoto = 5,
                DelayEntreReintentosMs = 300,
                Activo = true
            };

            target.ControllerContext = BuildControllerContext();
            var result = target.Modificar(model, new[] { 40 }) as RedirectToRouteResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestEliminar()
        {
            repositorioMock.Setup(r => r.Obtener<ConfigIdentificacionVehicular>(1)).Returns(configs[0]);

            target.ControllerContext = BuildControllerContext(isAjax: true);
            var result = target.Eliminar(1) as ContentResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Content, Is.EqualTo("true"));
            repositorioMock.Verify(r => r.Remover(It.Is<ConfigIdentificacionVehicular>(c => c.Id == 1)), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }
    }
}
