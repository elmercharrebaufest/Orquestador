using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.Extensions.Logging;

namespace Molinos.Orquest.Test.Servicios
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Unit Test"), TestFixture]
    public class ProcesadorFactoryTest
    {
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IProcesadorFactory> procesadorFactoryMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock;

        private Mock<IRepositorio> repositorioMock;

        private IKernel kernel;


        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            procesadorFactoryMock = new Mock<IProcesadorFactory>();
            driverFactoryMock = new Mock<IDriverFactory>();
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();
            
            kernel = new StandardKernel();
            kernel.Bind<IRepositorioFactory>().ToMethod(ctx => repositorioFactoryMock.Object);
            kernel.Bind<IProcesadorFactory>().ToMethod(ctx => procesadorFactoryMock.Object);
            kernel.Bind<IDriverFactory>().ToMethod(ctx => driverFactoryMock.Object);
            kernel.Bind<IAdministradorSuscripciones>().ToMethod(ctx => adminSuscripcionesMock.Object);
            kernel.Bind<ILogger>().ToMethod(ctx => new NullLogger());

            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

        }
        
        [Test]
        public void TestCrearFactory()
        {
            // Prueba que exista un procesador para cada comando y se inicializen sin errores
            //var kernel = new StandardKernel();
            Assert.That(() => new ProcesadorFactory(kernel, new NullLogger()), Throws.Nothing);
        }

        [Test]
        public void TestProcesadorParaDispositivoLogico()
        {
            repositorioMock.Setup(r => r.Listar<Dispositivo>(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo>());
            
            var target = new ProcesadorFactory(kernel, new NullLogger());

            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = new Dispositivo
                {
                    Id = 10,
                    Codigo = "ITCDM01",
                    EsConcentrador = true
                }
            };

            var procesador =  target.ProcesadorParaDispositivo(dispositivo, proc => { });
            Assert.That(procesador, Is.InstanceOf<ProcesadorDispositivoConcentrador>());
            Assert.That(procesador.CodigoDispositivo, Is.EqualTo("ITCDM01"));
        }

        [Test]
        public void TestProcesadorParaDispositivoFisico()
        {
            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.IsAny<Dispositivo>())).Returns(new DriverCabezalIPDummy());

            var target = new ProcesadorFactory(kernel, new NullLogger());

            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = null
            };

            var procesador = target.ProcesadorParaDispositivo(dispositivo, proc => { });
            Assert.That(procesador, Is.InstanceOf<ProcesadorDispositivoFisico>());
            Assert.That(procesador.CodigoDispositivo, Is.EqualTo("BALDM01"));
        }
    }
}
