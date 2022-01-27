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
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class CamaraControllerTest
    {
        private CamaraController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigCamara> camaras;
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
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverCamara>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new CamaraController(repositorioFactoryMock.Object, driverFactoryMock.Object, servicioMock.Object, new NullLogger());
            camaras = new List<ConfigCamara>
            {
                new ConfigCamara
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        Uri = "www.google.com",
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigCamara{Dispositivo = new Dispositivo()}
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
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCamara, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCamara>(camaras, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigCamara> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigCamara, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigCamara>(camaras, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigCamara> results = target.ViewBag.Items;
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
            var camara = new ConfigCamara
                {
                    Id = 1,
                    ClaseDriver = "Clase1",
                    Uri = "www.google.com",
                    Dispositivo = new Dispositivo {Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                };
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(camara) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCamara>(f => f.Dispositivo.Codigo == camara.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
        }


        [Test]
        public void TestCrearPostTieneConcentrador()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigCamara, bool>>>()))
                    .Returns<Expression<Func<ConfigCamara, bool>>>(q => camaras.Any((q.Compile())));
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(dispositivos[1].Id)).Returns(dispositivos[1]);

            var camara = new ConfigCamara
            {
                Id = 1,
                ClaseDriver = "Clase1",
                Uri = "www.google.com",
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1", EsConcentrador = false, ConcentradorId = dispositivos[1].Id }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(camara) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCamara>(f => f.Dispositivo.Codigo == camara.Dispositivo.Codigo && f.Dispositivo.Concentrador == dispositivos[1])), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));

            var camara = new ConfigCamara
            {
                Id = 1,
                ClaseDriver = "Clase1",
                Uri = "www.google.com",
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(camara) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCamara>(f => f.Dispositivo.Codigo == camara.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
        }

        [Test]
        public void TestModificar()
        {
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }



        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigCamara, bool>>>()))
                    .Returns<Expression<Func<ConfigCamara, bool>>>(q => camaras.Any((q.Compile())));
            camaras[0].Dispositivo.Configuracion = camaras[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(camaras[0].Dispositivo);

            var camara = new ConfigCamara
            {
                Id = 1,
                ClaseDriver = "Clase1",
                Uri = "www.google.com",
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(camara) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
        }



        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            camaras[0].Dispositivo.Configuracion = camaras[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(camaras[0].Dispositivo);

            var camara = new ConfigCamara
            {
                Id = 1,
                ClaseDriver = "Clase1",
                Uri = "www.google.com",
                Dispositivo = new Dispositivo { Id = 1, Activo = true, Codigo = "2", Descripcion = "Desc1" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(camara) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigCamara>(f => f.Dispositivo.Codigo == camara.Dispositivo.Codigo)), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigCamara>()), Times.Once());
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
            camaras[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigCamara>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarCamaraPorOrquestado));
        }

        [Test]
        public void TestEliminarLanzaException()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            repositorioMock.Setup(x => x.GuardarCambios()).Throws(new EntidadReferenciadaException());
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigCamara>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("El objeto esta siendo utilizado por otras entidades"));
        }

        [Test]
        public void TestProbar()
        {
            repositorioMock.Setup(x => x.Obtener<ConfigCamara>(It.IsAny<int>())).Returns(camaras[0]);
            var result = target.Probar(1) as ViewResult;

            repositorioMock.Verify(x => x.Obtener<ConfigCamara>(It.IsAny<int>()),Times.Once());
            Assert.That(result.Model, Is.TypeOf<ConfigCamara>());
            Assert.That(result.ViewName, Is.Null.Or.Empty);
        }

        [Test]
        public void TestTomarFoto()
        {
            servicioMock.Setup(x => x.Ejecutar(It.IsAny<EjecutarTomarFoto>())).Returns(new ResultadoTomarFoto(){Imagen = new byte[20]});
            var result = target.TomarFoto("1");

            servicioMock.Verify(x => x.Ejecutar(It.IsAny<EjecutarTomarFoto>()), Times.Once());
            Assert.That(((FileResult)result).ContentType, Is.TypeOf<string>());
            Assert.That(((FileResult)result).ContentType, Is.EqualTo("image/jpg"));
            Assert.That(((FileResult)result).FileDownloadName, Is.EqualTo(""));
        }
        /*public ActionResult TomarFoto(string codigo)
        {
            var resultado = (ResultadoTomarFoto) servicio.Ejecutar(new EjecutarTomarFoto
                {
                    CodigoDispositivo = codigo
                });
            return File(resultado.Imagen, "image/jpg");
        }*/
    }
}
