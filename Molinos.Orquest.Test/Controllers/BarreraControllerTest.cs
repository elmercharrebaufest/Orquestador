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
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
        "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class BarreraControllerTest
    {
        private BarreraController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigBarrera> barreraes;
        private List<Dispositivo> dispositivos;
        private Mock<IServicioOrquestador> servicioMock;
    
        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverBarrera>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new BarreraController(repositorioFactoryMock.Object, driverFactoryMock.Object, servicioMock.Object, new NullLogger());
            barreraes = new List<ConfigBarrera>
            {
                new ConfigBarrera
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        SenalDeActivacion = 0,
                        EstadoAbierta = false,
                        NumeroSalida = 1,
                        TiempoActivacion = 4,
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigBarrera{Dispositivo = new Dispositivo()}
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
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigBarrera, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigBarrera>(barreraes, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigBarrera> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigBarrera, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigBarrera>(barreraes, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigBarrera> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestCrear()
        {
            repositorioMock.Setup(s => s.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(new List<ConfigSensor> { new ConfigSensor { Id = 1,Dispositivo = new Dispositivo { Descripcion = "Disp 1" } }, new ConfigSensor { Id = 2, Dispositivo = new Dispositivo { Descripcion = "Disp 2" } } });

            var result = target.Crear() as ViewResult;
            var drivers = (List<SelectListItem>) result.ViewBag.Drivers;
            var dispositivos = (List<SelectListItem>)result.ViewBag.Concentradores;
            var sensores = new List<ConfigSensor>();
            
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(drivers.Count, Is.EqualTo(2));
            Assert.That(dispositivos.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestCrearPost()
        {
            var barrera = new ConfigBarrera
                {
                    Id = 1,
                    ClaseDriver = "Clase1",
                    SenalDeActivacion = 0,
                    EstadoAbierta = false,
                    NumeroSalida = 1,
                    TiempoActivacion = 4,
                    Dispositivo = new Dispositivo {Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(barrera) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBarrera>(f => f.Dispositivo.Codigo == barrera.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
        }


        [Test]
        public void TestCrearPostTieneConcentrador()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigBarrera, bool>>>()))
                    .Returns<Expression<Func<ConfigBarrera, bool>>>(q => barreraes.Any((q.Compile())));
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(dispositivos[1].Id)).Returns(dispositivos[1]);

            var barrera = new ConfigBarrera
            {
                Id = 1,
                ClaseDriver = "Clase1",
                SenalDeActivacion = 0,
                EstadoAbierta = false,
                NumeroSalida = 1,
                TiempoActivacion = 4,
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1", EsConcentrador = false, ConcentradorId = dispositivos[1].Id }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(barrera) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBarrera>(f => f.Dispositivo.Codigo == barrera.Dispositivo.Codigo && f.Dispositivo.Concentrador == dispositivos[1])), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            repositorioMock.Setup(s => s.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(new List<ConfigSensor> { new ConfigSensor { Id = 1, Dispositivo = new Dispositivo { Descripcion = "Disp 1" } }, new ConfigSensor { Id = 2, Dispositivo = new Dispositivo { Descripcion = "Disp 2" } } });


            var barrera = new ConfigBarrera
            {
                Id = 1,
                ClaseDriver = "Clase1",
                SenalDeActivacion = 0,
                EstadoAbierta = false,
                NumeroSalida = 1,
                TiempoActivacion = 4,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(barrera) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBarrera>(f => f.Dispositivo.Codigo == barrera.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
        }

        [Test]
        public void TestModificar()
        {
            repositorioMock.Setup(s => s.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(new List<ConfigSensor> { new ConfigSensor { Id = 1, Dispositivo = new Dispositivo { Descripcion = "Disp 1" } }, new ConfigSensor { Id = 2, Dispositivo = new Dispositivo { Descripcion = "Disp 2" } } });

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigBarrera>(It.IsAny<int>())).Returns(barreraes[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {
            repositorioMock.Setup(s => s.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(new List<ConfigSensor> { new ConfigSensor { Id = 1, Dispositivo = new Dispositivo { Descripcion = "Disp 1" } }, new ConfigSensor { Id = 2, Dispositivo = new Dispositivo { Descripcion = "Disp 2" } } });

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigBarrera>(It.IsAny<int>())).Returns(barreraes[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }



        [Test]
        public void TestModificarPost()
        {
            
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigBarrera, bool>>>()))
                    .Returns<Expression<Func<ConfigBarrera, bool>>>(q => barreraes.Any((q.Compile())));
            barreraes[0].Dispositivo.Configuracion = barreraes[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(barreraes[0].Dispositivo);

            var barrera = new ConfigBarrera
            {
                Id = 1,
                ClaseDriver = "Clase1",
                SenalDeActivacion = 0,
                EstadoAbierta = false,
                NumeroSalida = 1,
                TiempoActivacion = 4,
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(barrera) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
        }



        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Listar<ConfigSensor>(It.IsAny<Expression<Func<ConfigSensor, bool>>>()))
                .Returns(new List<ConfigSensor> { new ConfigSensor { Id = 1, Dispositivo = new Dispositivo { Descripcion = "Disp 1" } }, new ConfigSensor { Id = 2, Dispositivo = new Dispositivo { Descripcion = "Disp 2" } } });

            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            barreraes[0].Dispositivo.Configuracion = barreraes[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(barreraes[0].Dispositivo);

            var barrera = new ConfigBarrera
            {
                Id = 1,
                ClaseDriver = "Clase1",
                SenalDeActivacion = 0,
                EstadoAbierta = false,
                NumeroSalida = 1,
                TiempoActivacion = 4,
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "2", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(barrera) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBarrera>(f => f.Dispositivo.Codigo == barrera.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
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
            repositorioMock.Setup(s => s.Obtener<ConfigBarrera>(It.IsAny<int>())).Returns(barreraes[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigBarrera>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("true"));
        }

        [Test]
        public void TestEliminarOrquestadorTomado()
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
            barreraes[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigBarrera>(It.IsAny<int>())).Returns(barreraes[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigBarrera>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarBarreraPorOrquestado));
        }
    }
}
