using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Test.Mocks;
using Ninject;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class DriverFactoryTest
    {
        [Test]
        public void TestDriverExistente()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());

            var dispositivo = new Dispositivo
            {
                Codigo = "BALEM01",
                Configuracion = new ConfigCabezal { ClaseDriver = typeof(DriverCabezalMock).AssemblyQualifiedName }
            };

            var driver = target.Driver<IDriverCabezal>(dispositivo);
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<DriverCabezalMock>());
            Assert.That(((DriverCabezalMock)driver).Configurado, Is.True);
        }

        [Test]
        public void TestDriverExistentePeroTipoDispositivoIncorrecto()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());
            var dispositivo = new Dispositivo
            {
                Codigo = "BALEM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = typeof(DriverCabezalMock).AssemblyQualifiedName }
            };

            Assert.That(() => target.Driver<IDriverCabezal>(dispositivo), Throws.InstanceOf<TipoDispositivoIncorrectoException>());
        }

        [Test]
        public void TestDriverExistentePeroTipoDriverIncorrecto()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());
            var dispositivo = new Dispositivo
            {
                Codigo = "BALEM01",
                Configuracion = new ConfigCabezal { ClaseDriver = typeof(DriverHumedimetroMock).AssemblyQualifiedName }
            };

            Assert.That(() => target.Driver<IDriverCabezal>(dispositivo), Throws.InstanceOf<TipoDriverIncorrectoException>());
        }

        [Test]
        public void TestDriverNoExistente()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());

            var dispositivo = new Dispositivo
            {
                Codigo = "BALEM01",
                Configuracion = new ConfigCabezal { ClaseDriver = "MyNamespace.MyDriver" }
            };

            Assert.That(() => target.Driver<IDriverCabezal>(dispositivo), Throws.InstanceOf<DriverNoEncontradoException>());
        }

        [Test]
        public void TestDriverSubclaseDispExistente()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());

            var dispositivo = new Dispositivo
            {
                Codigo = "BALEM01",
                Configuracion = new SubclaseConfigCabezal { ClaseDriver = typeof(DriverCabezalMock).AssemblyQualifiedName }
            };

            var driver = target.Driver<IDriverCabezal>(dispositivo);
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<DriverCabezalMock>());
            Assert.That(((DriverCabezalMock)driver).Configurado, Is.True);
        }
    }

    internal class DriverCabezalMock : IDriverCabezal
    {
        public event EventHandler<EventoDriverEventArgs> EventoDriver;

        public bool Configurado { get; set; }
        public IEnumerable<string> EventosSoportados
        {
            get { return Enumerable.Empty<string>(); }
        }

        public ILogger Log { get; set; }

        public Type TipoDispositivo
        {
            get { return typeof(ConfigCabezal); }
        }

        public void DeshabilitarEventos()
        {
        }

        public void Dispose()
        {
        }

        public bool ForzarCero()
        {
            return false;
        }

        public void HabilitarEventos()
        {
        }

        public void InformarEstado()
        {
        }

        public void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            Configurado = true;
        }

        public bool MantenerConectado()
        {
            return false;
        }
        public decimal? ObtenerPeso()
        {
            return 1234;
        }

        public void VerificarDispositivo()
        {
        }
    }

    internal class DriverHumedimetroMock : IDriverHumedimetro
    {
        public event EventHandler<EventoDriverEventArgs> EventoDriver;

        public bool Configurado { get; set; }

        public IEnumerable<string> EventosSoportados
        {
            get { return Enumerable.Empty<string>(); }
        }

        public ILogger Log { get; set; }

        public Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public void DeshabilitarEventos()
        {
        }

        public virtual void Dispose()
        {
        }

        public void HabilitarEventos()
        {
        }

        public void InformarEstado()
        {
        }

        public void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            Configurado = true;
        }

        public bool MantenerConectado()
        {
            return false;
        }
        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            return 999;
        }

        public HumedimetroResultadoDto ObtenerHumedadPH(DateTime? fechaDeInicio = null)
        {
            Thread.Sleep(100);
            var humedimetro = new HumedimetroResultadoDto
            {
                Humedad = DateTime.Now.Millisecond % 100,
                PH = DateTime.Now.Millisecond % 100
            };
            return humedimetro;
        }

        public void VerificarDispositivo()
        {
        }
    }

    internal class SubclaseConfigCabezal : ConfigCabezal
    {
    }
}