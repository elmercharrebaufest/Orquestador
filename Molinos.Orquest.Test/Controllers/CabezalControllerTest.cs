using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
using Molinos.Orquest.Dominio.Resultados;
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
    public class CabezalControllerTest
    {
        private CabezalController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigCabezal> cabezales;
        private List<Dispositivo> dispositivos;
        private Mock<IServicioOrquestador>  servicioMock;


        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverCabezal>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new CabezalController(repositorioFactoryMock.Object, driverFactoryMock.Object, conversor, servicioMock.Object, new NullLogger());
            cabezales = new List<ConfigCabezal>
            {
                new ConfigCabezal
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        ComandoCereo = "C",
                        ComandoPeso = "P",
                        CantLecPesoEstable = 5,
                        CarInicioFrase = "|",
                        DireccionIp = "host",
                        IntLecCereo = 5,
                        IntLecPesoEstable = 5,
                        LongFrase = 5,
                        MaxCantLecPesoEstable = 5,
                        PosDesde = 1,
                        PosHasta = 5,
                        Puerto = 10,
                        TimeoutLectura = 10,
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigCabezal{Dispositivo = new Dispositivo()}
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
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCabezal, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCabezal>(cabezales, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigCabezalModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCabezal, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCabezal>(cabezales, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigCabezalModel> results = target.ViewBag.Items;
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
            var cabezal = new ConfigCabezalModel
                {
                    Id = 0,
                    ClaseDriver = "Clase2",
                    ComandoCereo = "C",
                    ComandoPeso = "P",
                    CantLecPesoEstable = 5,
                    CarInicioFrase = "|",
                    DireccionIp = "host",
                    IntLecCereo = 5,
                    IntLecPesoEstable = 5,
                    LongFrase = 5,
                    MaxCantLecPesoEstable = 5,
                    PosDesde = 1,
                    PosHasta = 5,
                    Puerto = 10,
                    TimeoutLectura = 10,
                    Dispositivo = new Dispositivo {Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2"}
                };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCabezal>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }


        [Test]
        public void TestCrearPostTieneConcentrador()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigCabezal, bool>>>()))
                    .Returns<Expression<Func<ConfigCabezal, bool>>>(q => cabezales.Any((q.Compile())));
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(dispositivos[1].Id)).Returns(dispositivos[1]);

            var cabezal = new ConfigCabezalModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoCereo = "C",
                ComandoPeso = "P",
                CantLecPesoEstable = 5,
                CarInicioFrase = "|",
                DireccionIp = "host",
                IntLecCereo = 5,
                IntLecPesoEstable = 5,
                LongFrase = 5,
                MaxCantLecPesoEstable = 5,
                PosDesde = 1,
                PosHasta = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2", EsConcentrador = false, ConcentradorId = dispositivos[1].Id}
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCabezal>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo && f.Dispositivo.Concentrador == dispositivos[1])), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));

            var cabezal = new ConfigCabezalModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoCereo = "C",
                ComandoPeso = "P",
                CantLecPesoEstable = 5,
                CarInicioFrase = "|",
                DireccionIp = "host",
                IntLecCereo = 5,
                IntLecPesoEstable = 5,
                LongFrase = 5,
                MaxCantLecPesoEstable = 5,
                PosDesde = 1,
                PosHasta = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(cabezal) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCabezal>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigCabezal>(It.IsAny<int>())).Returns(cabezales[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {
            cabezales[0].CarInicioFrase = "%02";
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigCabezal>(It.IsAny<int>())).Returns(cabezales[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Once());
        }



        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigCabezal, bool>>>()))
                    .Returns<Expression<Func<ConfigCabezal, bool>>>(q => cabezales.Any((q.Compile())));
            cabezales[0].Dispositivo.Configuracion = cabezales[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(cabezales[0].Dispositivo);

            var cabezal = new ConfigCabezalModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoCereo = "C",
                ComandoPeso = "P",
                CantLecPesoEstable = 5,
                CarInicioFrase = "|",
                DireccionIp = "host",
                IntLecCereo = 5,
                IntLecPesoEstable = 5,
                LongFrase = 5,
                MaxCantLecPesoEstable = 5,
                PosDesde = 1,
                PosHasta = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }



        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            cabezales[0].Dispositivo.Configuracion = cabezales[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(cabezales[0].Dispositivo);

            var cabezal = new ConfigCabezalModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoCereo = "C",
                ComandoPeso = "P",
                CantLecPesoEstable = 5,
                CarInicioFrase = "|",
                DireccionIp = "host",
                IntLecCereo = 5,
                IntLecPesoEstable = 5,
                LongFrase = 5,
                MaxCantLecPesoEstable = 5,
                PosDesde = 1,
                PosHasta = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "2", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(cabezal) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCabezal>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigCabezal>(It.IsAny<int>())).Returns(cabezales[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigCabezal>()), Times.Once());
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
            cabezales[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigCabezal>(It.IsAny<int>())).Returns(cabezales[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigCabezal>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarCabezalPorOrquestado));
        }

        [Test]
        public void TestProbar()
        {
            
            repositorioMock.Setup(s => s.Obtener<ConfigCabezal>(1)).Returns(cabezales[0]);
            var result = target.Probar(1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as ConfigCabezalModel;
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(cabezales[0].Id));
        }

        [Test]
        public void TestObtenerPesoOK()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarPesaje>(cmd => cmd.CodigoDispositivo == "BAL1")))
                        .Returns(new ResultadoEjecutar
                            {
                                Mensaje = Mensaje.ResultadoOK(),
                                Valores = new Dictionary<string, decimal> {{"Pesaje", 15000}}
                            });

            var result = target.ObtenerPeso("BAL1") as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
            Assert.That(model[0].Message, Is.EqualTo("15000"));
        }

        [Test]
        public void TestObtenerPesoErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarPesaje>(cmd => cmd.CodigoDispositivo == "BAL1")))
                        .Returns(new ResultadoEjecutar
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error"),
                            Valores = new Dictionary<string, decimal> ()
                        });

            var result = target.ObtenerPeso("BAL1") as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(new Mensaje(Codigos.DriverNoEncontrado, "error").ToString()));
        }

        [Test]
        public void TestObtenerPesoErrorServicio()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarPesaje>(cmd => cmd.CodigoDispositivo == "BAL1")))
                        .Throws<Exception>();

            var result = target.ObtenerPeso("BAL1") as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }
    }
}
