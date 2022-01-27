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
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Web.Controllers;
using Molinos.Scato.Dominio.Consultas;
using Moq;
using NUnit.Framework;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class SuscripcionControllerTest
    {
        private SuscripcionController target;
        private Mock<IServicioOrquestador> orquestadorServMock;
        private Mock<ILogger> logg;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private List<Suscripcion> suscripciones;
        [SetUp]
        public void SetUp()
        {
            orquestadorServMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            logg = new Mock<ILogger>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>())).Returns<Expression<Func<Suscripcion, bool>>>(q => suscripciones.Where(q.Compile()).ToList());
            target = new SuscripcionController(repositorioFactoryMock.Object, orquestadorServMock.Object, logg.Object);
            suscripciones = new List<Suscripcion>{
                new Suscripcion
                    {
                        Id = 1,
                        Dispositivo = new Dispositivo{ Descripcion = "dispositivo1", Codigo = "codigo1"}
                    },
                new Suscripcion
                    {
                        Id = 2,
                        Dispositivo = new Dispositivo{ Descripcion = "dispositivo2", Codigo = "codigo2"}
                    }
            };
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<Suscripcion>(suscripciones, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<Suscripcion> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Id, Is.EqualTo(1));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<Suscripcion>(suscripciones, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<Suscripcion> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Id, Is.EqualTo(1));
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
            repositorioMock.Setup(s => s.Obtener<Suscripcion>(It.IsAny<int>())).Returns(suscripciones[0]);
            orquestadorServMock.Setup(x => x.CancelarSuscripcion(It.IsAny<ComandoCancelarSuscripcion>())).Returns(new ResultadoCancelarSuscripcion { Mensaje = new Mensaje(0, "true") });
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            Assert.That(actual.Content, Is.EqualTo("true"));
        }

        [Test]
        public void TestEliminarError()
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
            repositorioMock.Setup(s => s.Obtener<Suscripcion>(It.IsAny<int>())).Returns(suscripciones[0]);
            orquestadorServMock.Setup(x => x.CancelarSuscripcion(It.IsAny<ComandoCancelarSuscripcion>())).Returns(new ResultadoCancelarSuscripcion{ Mensaje = new Mensaje(1,"error")});
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            Assert.That(actual.Content, Is.EqualTo("error"));
        }

        [Test]
        public void TestEliminarException()
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
            repositorioMock.Setup(s => s.Obtener<Suscripcion>(It.IsAny<int>())).Returns(suscripciones[0]);
            orquestadorServMock.Setup(x => x.CancelarSuscripcion(It.IsAny<ComandoCancelarSuscripcion>())).Throws(new EntidadReferenciadaException());
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            Assert.That(actual.Content, Is.EqualTo("El objeto esta siendo utilizado por otras entidades"));
        }

    }
}
