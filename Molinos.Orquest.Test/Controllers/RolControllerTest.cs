using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.Dominio.Seguridad;
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
    public class RolControllerTest
    {
        private RolController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private List<Rol> roles;
        private Mock<IServicioOrquestador> servicioMock;
        private List<RolPermisoOrquestador> permisosAsociados;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            target = new RolController(repositorioFactoryMock.Object, conversor, servicioMock.Object, new NullLogger());

            permisosAsociados = new List<RolPermisoOrquestador>()
            {
               new RolPermisoOrquestador()
               {
                   Id = 1,
                   PermisoOrquestador = PermisosOrquestador.AbmFirma,
                   Rol = new Rol(){Descripcion = "Rol1",Id = 4}
               }
            };

            roles = new List<Rol>
                {
                    new Rol
                        {
                            Id = 1,
                            Descripcion = "Usuario 1",
                            PermisosAsociados = permisosAsociados
                        },
                    new Rol
                        {
                            Id = 2,
                            Descripcion = "Usuario 2"
                        }
                };
        }


        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Rol, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<Rol>(roles, 1, 2, 2));

            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<Rol> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Descripcion, Is.EqualTo("Usuario 1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Rol, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<Rol>(roles, 1, 2, 2));

            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<Rol> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Descripcion, Is.EqualTo("Usuario 1"));
        }

        [Test]
        public void TestListarConFiltro()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Rol, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<Rol>(roles, 1, 2, 2));

            const string filter = "a";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<Rol> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Descripcion, Is.EqualTo("Usuario 1"));
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
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(false);
            var a = new List<PermisoModel>();
            a.Add(new PermisoModel());
            var arr = a.ToArray().ToJson();

            const string json = "[{\"Descripcion\":null,\"Id\":0}]";
            var result = target.Crear(new RolModel(), json) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(true);

            const string json = "[]";
            var result = target.Crear(new RolModel(), json) as ViewResult;

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(0));
            Assert.NotNull(result.Model);
            Assert.IsNull(result.View);
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestCrearPostInvalido2()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(false);

            var result = target.Crear(new RolModel(), "") as ViewResult;

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(0));
            Assert.NotNull(result.Model);
            Assert.IsNull(result.View);
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestModificar()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Obtener<Rol>(It.IsAny<int>()))
                .Returns(new Rol());

            var result = target.Modificar(1) as ViewResult;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.NotNull(result.Model);
        }

        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(false);
            repositorioMock.Setup(s => s.Obtener<Rol>(It.IsAny<int>()))
                .Returns(new Rol());

            const string json = "[{\"Descripcion\":null,\"Id\":0}]";
            var result = target.Modificar(new RolModel(), json) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);

        }

        [Test]
        public void TestModificarPostConRolYPermisosAsociados()
        {
            var permisosAsociados = new Collection<RolPermisoOrquestador>()
            {
                new RolPermisoOrquestador()
                {
                    Id = 1,
                    PermisoOrquestador = PermisosOrquestador.AbmFirma
                }
            };
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(false);
            repositorioMock.Setup(s => s.Obtener<Rol>(It.IsAny<int>()))
                .Returns(new Rol() { Descripcion = "Rol", Id = 1, PermisosAsociados = permisosAsociados });

            const string json = "[{\"Descripcion\":null,\"Id\":0}]";
            var result = target.Modificar(new RolModel(), json) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(1));
            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);

        }

        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Rol, bool>>>())).Returns(true);

            const string json = "[]";
            var result = target.Modificar(new RolModel(), json) as ViewResult;

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(0));
            Assert.NotNull(result.Model);
            Assert.IsNull(result.View);
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
            repositorioMock.Setup(s => s.Obtener<Usuario>(It.IsAny<int>())).Returns(new Usuario());
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<Rol>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("true"));
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
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(x => x.GuardarCambios()).Throws(new EntidadReferenciadaException());
            repositorioMock.Setup(s => s.Obtener<Usuario>(It.IsAny<int>())).Returns(new Usuario());
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<Rol>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("El objeto esta siendo utilizado por otras entidades"));
        }

        [Test]
        public void TestObtenerPermisos()
        {
            repositorioMock.Setup(x => x.Obtener<Rol>(It.IsAny<int>())).Returns(roles[0]);
            var result = target.ObtenerPermisos(1);
            
            Assert.That(result, Is.TypeOf<JsonResult>());
            Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));
            Assert.That(result.Data, Is.Not.Null);
        }
    }
}
