using System;
using System.IO;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Conversiones;
using Molinos.Orquest.Web.Models;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Controllers
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class FirmaControllerTest
    {
        private FirmaController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IConversor> conversorMock;
        private Mock<IServicioOrquestador> servicioMock;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            conversorMock = new Mock<IConversor>();

            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            target = new FirmaController(repositorioFactoryMock.Object, conversorMock.Object, servicioMock.Object, new NullLogger());
        }

        [Test]
        public void TestIndex()
        {
            Firma firma = new Firma()
            {
                Id = 1
            };

            FirmaModel firmaModel = new FirmaModel()
            {
                Id = 1
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true)).Returns(firma);
            conversorMock.Setup(x => x.Convertir<Firma, FirmaModel>(It.IsAny<Firma>())).Returns(firmaModel);
            var result = target.Index() as ViewResult;
            
            conversorMock.Verify(x => x.Convertir<Firma, FirmaModel>(It.IsAny<Firma>()),Times.Once());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
        }

        [Test]
        public void TestCrearFirmaExiste()
        {
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Id = 1
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true)).Returns(firma);
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            conversorMock.Verify(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()), Times.Once());
            conversorMock.Verify(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>()), Times.Never());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Never());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());

            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
        }

        [Test]
        public void TestCrearFirmaExisteSinLogo()
        {
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Id = 1
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true));
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Never());

            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestCrearFirmaNoExiste()
        {
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Logo = new byte[20],
                Id = 1,
                Favicon = new byte[20]
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true));
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            conversorMock.Verify(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()), Times.Never());
            conversorMock.Verify(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>()), Times.Once());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Once());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Once());

            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
        }

        [Test]
        public void TestCrearFirmaLanzaException()
        {
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Logo = new byte[20],
                Id = 1,
                Favicon = new byte[20]
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true)).Throws(new Exception());
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            conversorMock.Verify(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()), Times.Never());
            conversorMock.Verify(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>()), Times.Never());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Never());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Never());

            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestCrearFirmaConLogoFile()
        {
            Mock<HttpPostedFileBase> httpPostedFileBaseMock = new Mock<HttpPostedFileBase>();
            httpPostedFileBaseMock.Setup(x => x.InputStream).Returns(new BufferedStream(Stream.Synchronized(Stream.Null)));
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Logo = new byte[20],
                Id = 1,
                Favicon = new byte[20],
                FaviconFile = httpPostedFileBaseMock.Object
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true)).Throws(new Exception());
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            conversorMock.Verify(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()), Times.Never());
            conversorMock.Verify(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>()), Times.Never());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Never());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Never());

            Assert.That(firmaModel.FaviconFile, Is.Null);
            Assert.That(firmaModel.Favicon, Is.TypeOf<Byte[]>());
            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestCrearFirmaConFaviconFile()
        {
            Mock<HttpPostedFileBase> httpPostedFileBaseMock = new Mock<HttpPostedFileBase>();
            httpPostedFileBaseMock.Setup(x => x.InputStream).Returns(new BufferedStream(Stream.Synchronized(Stream.Null)));
            Firma firma = new Firma()
            {
                Id = 1
            };
            FirmaModel firmaModel = new FirmaModel()
            {
                Logo = new byte[20],
                Id = 1,
                Favicon = new byte[20],
                LogoFile = httpPostedFileBaseMock.Object
            };
            repositorioMock.Setup(x => x.Obtener<Firma>(f => true)).Throws(new Exception());
            conversorMock.Setup(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>())).Returns(firma);
            conversorMock.Setup(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()));

            var result = target.Index(firmaModel) as ViewResult;

            conversorMock.Verify(x => x.Convertir(It.IsAny<FirmaModel>(), It.IsAny<Firma>()), Times.Never());
            conversorMock.Verify(x => x.Convertir<FirmaModel, Firma>(It.IsAny<FirmaModel>()), Times.Never());
            repositorioMock.Verify(x => x.Obtener<Firma>(f => true), Times.Once());
            repositorioMock.Verify(x => x.Agregar(It.IsAny<Firma>()), Times.Never());
            repositorioMock.Verify(x => x.GuardarCambios(), Times.Never());

            Assert.That(firmaModel.LogoFile, Is.Null);
            Assert.That(firmaModel.Logo, Is.TypeOf<Byte[]>());
            Assert.That(result.ViewName, Is.Not.Null.Or.Empty);
            Assert.That(result.ViewName, Is.EqualTo("Index"));
            Assert.That(result.Model, Is.TypeOf<FirmaModel>());
            Assert.That(result.ViewData.ModelState.IsValid, Is.EqualTo(false));
        }

        [Test]
        public void TestObtenerLogo()
        {
            repositorioMock.Setup(x => x.Obtener(It.IsAny<Expression<Func<Firma, bool>>>()))
                .Returns(new Firma() {Id = 1});
            var result = target.Logo();

            repositorioMock.Verify(x => x.Obtener(It.IsAny<Expression<Func<Firma, bool>>>()),Times.Once());
            Assert.That(result.FileContents, Is.TypeOf<Byte[]>());
            Assert.That(result.ContentType, Is.TypeOf<string>());
        }

        [Test]
        public void TestObtenerFavicon()
        {
            repositorioMock.Setup(x => x.Obtener(It.IsAny<Expression<Func<Firma, bool>>>()))
                .Returns(new Firma() { Id = 1 });
            var result = target.Favicon();

            repositorioMock.Verify(x => x.Obtener(It.IsAny<Expression<Func<Firma, bool>>>()), Times.Once());
            Assert.That(result.ContentType, Is.TypeOf<string>());
            Assert.That(result.FileContents, Is.TypeOf<Byte[]>());
        }
    }
}
