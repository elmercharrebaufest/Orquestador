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
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class ItcControllerTest
    {
        private ItcController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigItc> Itces;
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
            conversor = FactoryConversor.ConversorAutoMapper;
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverItc>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new ItcController(repositorioFactoryMock.Object, driverFactoryMock.Object, conversor, servicioMock.Object, new NullLogger());
            Itces = new List<ConfigItc>
            {
                new ConfigItc
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        CarInicioFrase = "|",
                        DireccionIp = "host",
                        LongFrase = 5,
                        Puerto = 10,
                        TimeoutLectura = 10,
                        CarFinFrase = " ",
                        ComandoActivarSalida = "3",
                        IntervaloPolling = 2,
                        ComandoEstado = "3",
                        ComandoTarjeta = "T",
                        DelimitadorCampos = ",",
                        RespuestaError = "E",
                        RespuestaExito = "X",
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigItc{Dispositivo = new Dispositivo()}
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
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigItc, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigItc>(Itces, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigItcModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigItc, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigItc>(Itces, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigItcModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestCrear()
        {
            var result = target.Crear() as ViewResult;
            var drivers = (List<SelectListItem>) result.ViewBag.Drivers;
            var dispositivos = (List<SelectListItem>)result.ViewBag.Concentradores;
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(drivers.Count, Is.EqualTo(2));
            Assert.That(dispositivos.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestCrearPost()
        {
            var itc = new ConfigItcModel
                {
                    Id = 1,
                    ClaseDriver = "Clase1",
                    CarInicioFrase = "|",
                    DireccionIp = "host",
                    LongFrase = 5,
                    Puerto = 10,
                    TimeoutLectura = 10,
                    CarFinFrase = " ",
                    ComandoActivarSalida = "3",
                    IntervaloPolling = 2,
                    ComandoEstado = "3",
                    ComandoTarjeta = "T",
                    DelimitadorCampos = ",",
                    RespuestaError = "E",
                    RespuestaExito = "X",
                    Dispositivo = new Dispositivo {Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1", ServerFijo = ""}
                };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(itc) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigItc>(f => f.Dispositivo.Codigo == itc.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Exactly(4));
        }


        [Test]
        public void TestCrearPostTieneConcentrador()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigItc, bool>>>()))
                    .Returns<Expression<Func<ConfigItc, bool>>>(q => Itces.Any((q.Compile())));
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(dispositivos[1].Id)).Returns(dispositivos[1]);

            var itc = new ConfigItcModel
            {
                Id = 1,
                ClaseDriver = "Clase1",
                CarInicioFrase = "|",
                DireccionIp = "host",
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                CarFinFrase = " ",
                ComandoActivarSalida = "3",
                IntervaloPolling = 2,
                ComandoEstado = "3",
                ComandoTarjeta = "T",
                DelimitadorCampos = ",",
                RespuestaError = "E",
                RespuestaExito = "X",
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2", EsConcentrador = false, ConcentradorId = dispositivos[1].Id, ServerFijo = "" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(itc) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigItc>(f => f.Dispositivo.Codigo == itc.Dispositivo.Codigo && f.Dispositivo.Concentrador == dispositivos[1])), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));

            var itc = new ConfigItcModel
            {
                Id = 1,
                ClaseDriver = "Clase1",
                CarInicioFrase = "|",
                DireccionIp = "host",
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                CarFinFrase = " ",
                ComandoActivarSalida = "3",
                IntervaloPolling = 2,
                ComandoEstado = "3",
                ComandoTarjeta = "T",
                DelimitadorCampos = ",",
                RespuestaError = "E",
                RespuestaExito = "X",
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(itc) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigItc>(f => f.Dispositivo.Codigo == itc.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
        }

        [Test]
        public void TestModificar()
        {
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigItc>(It.IsAny<int>())).Returns(Itces[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {
            Itces[0].CarInicioFrase = "%02";
            Itces[0].CarFinFrase = "%02";
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigItc>(It.IsAny<int>())).Returns(Itces[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Exactly(2));
        }



        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigItc, bool>>>()))
                    .Returns<Expression<Func<ConfigItc, bool>>>(q => Itces.Any((q.Compile())));
            Itces[0].Dispositivo.Configuracion = Itces[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(Itces[0].Dispositivo);

            var itc = new ConfigItcModel
            {
                Id = 1,
                ClaseDriver = "Clase1",
                CarInicioFrase = "|",
                DireccionIp = "host",
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                CarFinFrase = " ",
                ComandoActivarSalida = "3",
                IntervaloPolling = 2,
                ComandoEstado = "3",
                ComandoTarjeta = "T",
                DelimitadorCampos = ",",
                RespuestaError = "E",
                RespuestaExito = "X",
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "2", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(itc) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Exactly(3));
        }



        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            Itces[0].Dispositivo.Configuracion = Itces[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(Itces[0].Dispositivo);

            var itc = new ConfigItcModel
            {
                Id = 1,
                ClaseDriver = "Clase1",
                CarInicioFrase = "|",
                DireccionIp = "host",
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                CarFinFrase = " ",
                ComandoActivarSalida = "3",
                IntervaloPolling = 2,
                ComandoEstado = "3",
                ComandoTarjeta = "T",
                DelimitadorCampos = ",",
                RespuestaError = "E",
                RespuestaExito = "X",
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "2", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(itc) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigItc>(f => f.Dispositivo.Codigo == itc.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigItc>(It.IsAny<int>())).Returns(Itces[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigItc>()), Times.Once());
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
            Itces[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigItc>(It.IsAny<int>())).Returns(Itces[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigItc>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarItcPorOrquestado));
        }
    }
}
