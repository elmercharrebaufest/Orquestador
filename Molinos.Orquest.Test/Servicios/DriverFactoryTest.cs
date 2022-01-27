using System;
using System.Collections.Generic;
using System.Linq;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Test.Mocks;
using NUnit.Framework;
using Ninject;
using Ninject.Extensions.Logging;
using Ninject.Planning.Bindings;

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
                    Configuracion = new ConfigCabezal {ClaseDriver = typeof (DriverCabezalMock).AssemblyQualifiedName}
                };
                 
            var driver = target.Driver<IDriverCabezal>(dispositivo);
            Assert.That(driver, Is.Not.Null);
            Assert.That(driver, Is.InstanceOf<DriverCabezalMock>());
            Assert.That(((DriverCabezalMock) driver).Configurado, Is.True);
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

        [Test]
        public void TestDriverNoExistente()
        {
            var kernel = new StandardKernel();
            kernel.Bind<ILoggerFactory>().To<NullLoggerFactory>();

            var target = new DriverFactory(kernel, new NullLogger());

            
            var dispositivo = new Dispositivo
                {
                    Codigo = "BALEM01",
                    Configuracion = new ConfigCabezal {ClaseDriver = "MyNamespace.MyDriver"}
                };

            Assert.That(() => target.Driver<IDriverCabezal>(dispositivo), Throws.InstanceOf<DriverNoEncontradoException>());
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
                    Configuracion = new ConfigHumedimetro {ClaseDriver = typeof (DriverCabezalMock).AssemblyQualifiedName}
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
                    Configuracion =  new ConfigCabezal { ClaseDriver = typeof(DriverHumedimetroMock).AssemblyQualifiedName }
                };

            Assert.That(() => target.Driver<IDriverCabezal>(dispositivo), Throws.InstanceOf<TipoDriverIncorrectoException>());
        }
    }

    class SubclaseConfigCabezal : ConfigCabezal
    {
        
    }

    class DriverCabezalMock : IDriverCabezal
    {
        public bool Configurado { get; set; }

        public event EventHandler<EventoDriverEventArgs> EventoDriver;

        public bool MantenerConectado()
        {
            return false;
        }

        public Type TipoDispositivo
        {
            get { return typeof(ConfigCabezal); }
        }

        public IEnumerable<string> EventosSoportados
        {
            get { return Enumerable.Empty<string>(); }
        }

        public void HabilitarEventos()
        {
        }

        public void DeshabilitarEventos()
        {
        }

        public void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            Configurado = true;
        }

        public void VerificarDispositivo()
        {
        }

        public void InformarEstado()
        {
        }

        public ILogger Log { get; set; }

        public decimal? ObtenerPeso()
        {
            return 1234;
        }

        public bool ForzarCero()
        {
            return false;
        }

        public void Dispose()
        {
        }
    }

    class DriverHumedimetroMock : IDriverHumedimetro
    {
        public bool Configurado { get; set; }

        public bool MantenerConectado()
        {
            return false;
        }

        public event EventHandler<EventoDriverEventArgs> EventoDriver;

        public Type TipoDispositivo
        {
            get { return typeof(ConfigHumedimetro); }
        }

        public IEnumerable<string> EventosSoportados
        {
            get { return Enumerable.Empty<string>(); }
        }

        public void HabilitarEventos()
        {
        }

        public void DeshabilitarEventos()
        {
        }

        public void Inicializar(string codigo, ConfigDispositivo configuracion)
        {
            Configurado = true;
        }

        public void VerificarDispositivo()
        {
        }

        public void InformarEstado()
        {
        }

        public ILogger Log { get; set; }

        public decimal? ObtenerHumedad(DateTime? fechaDeInicio = null)
        {
            return 999;
        }

        public virtual void Dispose()
        {
        }
    }
}
