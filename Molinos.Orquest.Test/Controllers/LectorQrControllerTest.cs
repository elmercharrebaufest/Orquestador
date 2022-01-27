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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class LectorQrControllerTest
    {
        private LectorQrController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigLectorQr> lectoresQr;
        private List<Dispositivo> dispositivos;
        private Mock<IServicioOrquestador> servicioMock;


        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverLectorQr>()).Returns(new List<string> { "DriverPantallapuestoVianda1", "DriverPantallapuestoVianda2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new LectorQrController(driverFactoryMock.Object, conversor, repositorioFactoryMock.Object, servicioMock.Object, new NullLogger());

            lectoresQr = new List<ConfigLectorQr>
            {
                new ConfigLectorQr
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    },
                new ConfigLectorQr{Dispositivo = new Dispositivo()}
            };
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigPuestoDeVianda, bool>>>()))
                .Returns(new List<ConfigPuestoDeVianda>());
            dispositivos = new List<Dispositivo>
                {
                    new Dispositivo{Codigo = "1", Descripcion = "11", EsConcentrador = true, Id = 1},
                    new Dispositivo{Codigo = "2", Descripcion = "22", Id = 2}
                };
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigLectorQr, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<ConfigLectorQr>(lectoresQr, 1, 2, 2));


            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigLectorQr> results = target.ViewBag.Items;


            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 0 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock
                .Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigLectorQr, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<ConfigLectorQr>(lectoresQr, 1, 2, 2));

            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigLectorQr> results = target.ViewBag.Items;


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
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));

            var puestoVianda = new ConfigLectorQr
            {
                Id = 0,
                ClaseDriver = "Clase2",
                //DireccionIp = "host",
                //Puerto = 10,
                //TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "2", Descripcion = "Desc2" }
            };


            var context = new Mock<HttpContextBase>();
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(puestoVianda) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigLectorQr>(f => f.Dispositivo.Codigo == puestoVianda.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
        }

        [Test]
        public void TestModificar()
        {

            var context = new Mock<HttpContextBase>();
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigLectorQr>(It.IsAny<int>()))
                .Returns(new ConfigLectorQr() { Dispositivo = new Dispositivo() });
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestModificarConSpecialCaracter()
        {

            var context = new Mock<HttpContextBase>();
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigLectorQr>(It.IsAny<int>()))
                .Returns(new ConfigLectorQr() { Dispositivo = new Dispositivo() });
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }



        [Test]
        public void TestModificarPost()
        {
            lectoresQr[0].Dispositivo.Configuracion = lectoresQr[0];
            repositorioMock.Setup(s => s.Obtener<ConfigLectorQr>(It.IsAny<int>()))
                .Returns(new ConfigLectorQr { Dispositivo = lectoresQr[0].Dispositivo });
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(lectoresQr[0].Dispositivo);
            repositorioMock.Setup(s => s.Obtener<ConfigPuestoDeVianda>(It.IsAny<int>())).Returns(new ConfigPuestoDeVianda());

            var cabezal = new ConfigLectorQr
            {
                Id = 0,
                ClaseDriver = "Clase2",
                //DireccionIp = "host",
                //Puerto = 10,
                //TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };

            var context = new Mock<HttpContextBase>();
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(cabezal) as ContentResult;
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
            lectoresQr[0].Dispositivo.Configuracion = lectoresQr[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(lectoresQr[0].Dispositivo);

            var puestoVianda = new ConfigLectorQr
            {
                Id = 0,
                ClaseDriver = "Clase2",
                //DireccionIp = "host",
                //Puerto = 10,
                //TimeoutLectura = 10,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "2", Descripcion = "Desc2" }
            };

            var context = new Mock<HttpContextBase>();

            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);


            var result = target.Modificar(puestoVianda) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigLectorQr>(f => f.Dispositivo.Codigo == puestoVianda.Dispositivo.Codigo)), Times.Never());
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
            repositorioMock.Setup(s => s.Obtener<ConfigLectorQr>(It.IsAny<int>())).Returns(lectoresQr[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigLectorQr>()), Times.Once());
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
            lectoresQr[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigLectorQr>(It.IsAny<int>())).Returns(lectoresQr[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigLectorQr>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarLectorQrPorOrquestado));
        }
    }
}

