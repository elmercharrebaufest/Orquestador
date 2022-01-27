using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
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
    public class UsuarioControllerTest
    {
        private UsuarioController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private List<Usuario> usuarios;
        private Mock<IServicioOrquestador> servicioMock;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            target = new UsuarioController(repositorioFactoryMock.Object, conversor, servicioMock.Object, new NullLogger());

            usuarios = new List<Usuario>
                {
                    new Usuario
                        {
                            Id = 1,
                            NombreUsuario = "Usuario 1",
                            RolesAsociados = new List<Rol>()
                            {
                                new Rol()
                                {
                                    Descripcion = "Rol1",
                                    Id = 1
                                }
                            }
                        },
                    new Usuario
                        {
                            Id = 2,
                            NombreUsuario = "Usuario 2"
                        }
                };
        }


        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Usuario,bool>>>(),It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<Usuario>(usuarios, 1, 2, 2));

            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<Usuario> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].NombreUsuario, Is.EqualTo("Usuario 1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<Usuario, bool>>>(), It.IsAny<Paginacion>()))
                .Returns(new ListaPaginada<Usuario>(usuarios, 1, 2, 2));

            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<Usuario> results = target.ViewBag.Items;

            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1, 2 }));
            Assert.That((object)target.ViewBag.Items.Items[0].NombreUsuario, Is.EqualTo("Usuario 1"));
        }

        [Test]
        public void TestCrear()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });

            var result = target.Crear() as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestCrearPost()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Usuario, bool>>>())).Returns(false);

            const string json = "[]";
            var result = target.Crear(new Usuario(), json) as ContentResult;
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
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Usuario, bool>>>())).Returns(true);

            const string json = "[]";
            var result = target.Crear(new Usuario(), json) as ViewResult;

            repositorioMock.Verify(p => p.GuardarCambios(), Times.Exactly(0));
            Assert.NotNull(result.Model);
            Assert.IsNull(result.View);
        }

        [Test]
        public void TestModificar()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Obtener<Usuario>(It.IsAny<int>()))
                .Returns(new Usuario());

            var result = target.Modificar(1) as ViewResult;

            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.NotNull(result.Model);
        }

        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Listar<Rol>(It.IsAny<Expression<Func<Rol, bool>>>()))
                .Returns(new List<Rol> { new Rol { Id = 1, Descripcion = "Rol 1" }, new Rol { Id = 2, Descripcion = "Rol 2" } });
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Usuario, bool>>>())).Returns(false);
            repositorioMock.Setup(s => s.Obtener<Usuario>(It.IsAny<int>()))
                .Returns(new Usuario());

            const string json = "[]";
            var result = target.Modificar(new Usuario(), json) as ContentResult;
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
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Usuario, bool>>>())).Returns(true);

            const string json = "[]";
            var result = target.Modificar(new Usuario(), json) as ViewResult;

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
            repositorioMock.Verify(v => v.Remover(It.IsAny<Usuario>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("true"));

        }

        [Test]
        public void TestEliminarConExcepcion()
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
            repositorioMock.Setup(s => s.Obtener<Usuario>(It.IsAny<int>())).Returns(new Usuario());
            repositorioMock.Setup(s => s.GuardarCambios()).Throws(new EntidadReferenciadaException());

            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<Usuario>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("El objeto esta siendo utilizado por otras entidades"));
        }

        [Test]
        public void TestObtenerRoles()
        {
            repositorioMock.Setup(x => x.Obtener<Usuario>(It.IsAny<int>())).Returns(usuarios[0]);
            var rol = conversor.Convertir<Rol, RolModel>(((List<Rol>)usuarios[0].RolesAsociados)[0]);

            var result = target.ObtenerRoles(1) as JsonResult;

            Assert.That(result, Is.TypeOf<JsonResult>());
            Assert.That(result.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));
            Assert.That(((List<RolModel>)result.Data)[0].Descripcion, Is.EqualTo("Rol1"));
            Assert.That(((List<RolModel>)result.Data)[0].Id, Is.EqualTo(1));
        }
    }
}
