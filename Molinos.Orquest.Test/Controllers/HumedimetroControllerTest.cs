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
    public class HumedimetroControllerTest
    {
        private HumedimetroController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private IConversor conversor;
        private List<ConfigHumedimetro> humedimetros;
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
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverHumedimetro>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new HumedimetroController(repositorioFactoryMock.Object, driverFactoryMock.Object, conversor, servicioMock.Object, new NullLogger());
            humedimetros = new List<ConfigHumedimetro>
            {
                new ConfigHumedimetro
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        ComandoHumedad = "C",
                        DelimitadorCampos = "|",
                        DireccionIp = "host",
                        PosicionCampoHumedad = 5,
                        LongFrase = 5,
                        Puerto = 10,
                        TimeoutLectura = 10,
                        Dispositivo = new Dispositivo{Id = 0, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }
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
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigHumedimetro>(humedimetros, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigHumedimetroModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigHumedimetro>(humedimetros, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigHumedimetroModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestCrear()
        {
            var result = target.Crear() as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestCrearPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>()))
                    .Returns<Expression<Func<ConfigHumedimetro, bool>>>(q => humedimetros.Any((q.Compile())));

            var cabezal = new ConfigHumedimetroModel
                {
                    Id = 0,
                    ClaseDriver = "Clase2",
                    ComandoHumedad = "C",
                    DelimitadorCampos = "|",
                    DireccionIp = "host",
                    PosicionCampoHumedad = 5,
                    LongFrase = 5,
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
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigHumedimetro>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));

            var humedimetro = new ConfigHumedimetroModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoHumedad = "C",
                DelimitadorCampos = "|",
                DireccionIp = "host",
                PosicionCampoHumedad = 5,
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(humedimetro) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigHumedimetro>(f => f.Dispositivo.Codigo == humedimetro.Dispositivo.Codigo)), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigHumedimetro>(It.IsAny<int>())).Returns(humedimetros[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {
            humedimetros[0].DelimitadorCampos = "%02";
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigHumedimetro>(It.IsAny<int>())).Returns(humedimetros[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Once());
        }



        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>()))
                    .Returns<Expression<Func<ConfigHumedimetro, bool>>>(q => humedimetros.Any((q.Compile())));
            humedimetros[0].Dispositivo.Configuracion = humedimetros[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(humedimetros[0].Dispositivo);

            var cabezal = new ConfigHumedimetroModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoHumedad = "C",
                DelimitadorCampos = "|",
                DireccionIp = "host",
                PosicionCampoHumedad = 5,
                LongFrase = 5,
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
            humedimetros[0].Dispositivo.Configuracion = humedimetros[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(humedimetros[0].Dispositivo);

            var cabezal = new ConfigHumedimetroModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoHumedad = "C",
                DelimitadorCampos = "|",
                DireccionIp = "host",
                PosicionCampoHumedad = 5,
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(cabezal) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigHumedimetro>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Humedimetro_CodigoExistente));
        }

        [Test]
        public void TestCrearPostTieneConcentrador()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>()))
                    .Returns<Expression<Func<ConfigHumedimetro, bool>>>(q => humedimetros.Any((q.Compile())));
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(dispositivos[1].Id)).Returns(dispositivos[1]);

            var cabezal = new ConfigHumedimetroModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                ComandoHumedad = "C",
                DelimitadorCampos = "|",
                DireccionIp = "host",
                PosicionCampoHumedad = 5,
                LongFrase = 5,
                Puerto = 10,
                TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "33", Descripcion = "Desc2", EsConcentrador = false, ConcentradorId = dispositivos[1].Id }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigHumedimetro>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo && f.Dispositivo.Concentrador == dispositivos[1])), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
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
            repositorioMock.Setup(s => s.Obtener<ConfigHumedimetro>(It.IsAny<int>())).Returns(humedimetros[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigHumedimetro>()), Times.Once());
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
            humedimetros[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigHumedimetro>(It.IsAny<int>())).Returns(humedimetros[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigHumedimetro>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarHumedimetroPorOrquestado));
        }

        [Test]
        public void TestProbar()
        {

            repositorioMock.Setup(s => s.Obtener<ConfigHumedimetro>(1)).Returns(humedimetros[0]);
            var result = target.Probar(1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as ConfigHumedimetroModel;
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(humedimetros[0].Id));
        }

        [Test]
        public void TestObtenerPesoOK()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarAnalisisHumedad>(cmd => cmd.CodigoDispositivo == "HUM1")))
                        .Returns(new ResultadoEjecutar
                        {
                            Mensaje = Mensaje.ResultadoOK(),
                            Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12 } }
                        });

            var result = target.ObtenerHumedad("HUM1",1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
            Assert.That(model[0].Message, Is.EqualTo("12"));
        }

        [Test]
        public void TestObtenerPesoErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarAnalisisHumedad>(cmd => cmd.CodigoDispositivo == "HUM1")))
                        .Returns(new ResultadoEjecutar
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error"),
                            Valores = new Dictionary<string, decimal>()
                        });

            var result = target.ObtenerHumedad("HUM1",1) as ViewResult;
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
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarAnalisisHumedad>(cmd => cmd.CodigoDispositivo == "HUM1")))
                        .Throws<Exception>();

            var result = target.ObtenerHumedad("HUM1",1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }
    }
}
