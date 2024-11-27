using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class JsonToIotBoxControllerTest
    {
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigJsonToIotBox> Jsons;
        private List<Dispositivo> dispositivos;
        private List<FormatosJson> formatos;
        private Mock<IServicioOrquestador> servicioMock;
        private JsonToIotBoxController target;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverJsonToIotBox>()).Returns(new List<string> { "Driver1", "Driver2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new JsonToIotBoxController(repositorioFactoryMock.Object, driverFactoryMock.Object, servicioMock.Object, new NullLogger());
            Jsons = new List<ConfigJsonToIotBox>
            {
                new ConfigJsonToIotBox
                {
                    Id = 1,
                    ClaseDriver = "Driver1",
                    NumeroSalida = 1,
                    FormatoJson = 1,
                    Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1" }
                },
                new ConfigJsonToIotBox{ Dispositivo = new Dispositivo()}
            };

            dispositivos = new List<Dispositivo>
            {
                new Dispositivo{Codigo = "1", Descripcion = "11", EsConcentrador = true, Id = 1},
                new Dispositivo{Codigo = "2", Descripcion = "22", Id = 2}
            };

            formatos = new List<FormatosJson>
            {
                new FormatosJson{Id = 1, Dispositivo_nombre = "Formato1"},
                new FormatosJson{Id = 2, Dispositivo_nombre = "Formato2"}
            };
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigJsonToIotBox, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigJsonToIotBox>(Jsons, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigJsonToIotBox> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigJsonToIotBox, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigJsonToIotBox>(Jsons, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigJsonToIotBox> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestCrearGet()
        {
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<FormatosJson, bool>>>())).Returns<Expression<Func<FormatosJson, bool>>>(q => formatos);

            var result = target.Crear() as ViewResult;
            var drivers = (List<SelectListItem>)result.ViewBag.Drivers;
            var dispositivos = (List<SelectListItem>)result.ViewBag.Concentradores;
            var sensores = (List<SelectListItem>)result.ViewBag.Sensores;
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(drivers.Count, Is.EqualTo(2));
            Assert.That(dispositivos.Count, Is.EqualTo(2));
            Assert.That(sensores.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestCrearPost()
        {
            var json = new ConfigJsonToIotBox
            {
                Id = 0,
                ClaseDriver = "Driver1",
                NumeroSalida = 1,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(json) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigJsonToIotBox>(f => f.Dispositivo.Codigo == json.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            //server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestModificarGet()
        {
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<FormatosJson, bool>>>())).Returns<Expression<Func<FormatosJson, bool>>>(q => formatos);

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigJsonToIotBox>(It.IsAny<int>())).Returns(Jsons[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigCabezal, bool>>>()))
                .Returns<Expression<Func<ConfigJsonToIotBox, bool>>>(q => Jsons.Any((q.Compile())));
            Jsons[0].Dispositivo.Configuracion = Jsons[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(Jsons[0].Dispositivo);
            var json = new ConfigJsonToIotBox
            {
                Id = 0,
                ClaseDriver = "Driver1",
                NumeroSalida = 1,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(json) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
            //server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestEliminar()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigJsonToIotBox>(It.IsAny<int>())).Returns(Jsons[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigJsonToIotBox>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("true"));
        }
    }
}