using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Procesamiento
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class ProcesadorDispositivoTest
    {
        private ProcesadorDispositivoFisico target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IProcesadorFactory> procesadorFactoryMock;
        private Mock<IProcesadorComando> procesadorMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private Mock<IDriver> driverMock;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock; 

        private Dispositivo dispositivo;

        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioMock = new Mock<IRepositorio>();
            procesadorFactoryMock = new Mock<IProcesadorFactory>();
            procesadorMock = new Mock<IProcesadorComando>();
            driverFactoryMock = new Mock<IDriverFactory>();
            driverMock = new Mock<IDriver>();
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();

            procesadorFactoryMock.Setup(factory => factory.ProcesadorPara(It.IsAny<Comando>())).Returns(procesadorMock.Object);
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                   .Returns<Expression<Func<Dispositivo, bool>>>(
                       condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.IsAny<Dispositivo>())).Returns(driverMock.Object);
        }

        [Test]
        public void TestProcesarComandoConColaVacia()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            dispositivo = new Dispositivo
                {
                    Codigo = "BALDM01",
                    Configuracion = new ConfigCabezal(),
                    Activo = true
                };
            
            var eventoFin = new AutoResetEvent(false);
            
            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object, 
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp =>
                    {
                        dispositivoLiberado = disp.ProcesarRemanentes();
                        eventoFin.Set();
                    }, new NullLogger());


            var resultado = target.Procesar(new EjecutarPesaje { CodigoDispositivo = "BALDM01" }) as ResultadoEjecutar;
            
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.True);
        }

        [Test]
        public void TestProcesarDispositivoIncorrecto()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            var eventoFin = new AutoResetEvent(false);
            bool dispositivoLiberado = false;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp =>
                    {
                        dispositivoLiberado = disp.ProcesarRemanentes();
                        eventoFin.Set();
                    }, new NullLogger());

            Assert.That(() => target.Procesar(new EjecutarCereoCabezal {CodigoDispositivo = "BALDM99"}),
                        Throws.InstanceOf<ProcesadorException>());

            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.True);
        }

        //[Test]
        public void TestProcesarVariosComandos()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                    {
                        Thread.Sleep(30);
                        return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                    });

            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            string dispositivoLiberado = null;
            var liberaciones = 0;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object, driverFactoryMock.Object, adminSuscripcionesMock.Object, disp =>
                {
                    dispositivoLiberado = disp.CodigoDispositivo;
                    liberaciones++;
                    disp.ProcesarRemanentes();
                }, new NullLogger());


            var resultados = Enumerable.Range(0, 10)
                                       .Select((x, y) => new EjecutarPesaje {CodigoDispositivo = "BALDM01"})
                                       .ToList()
                                       .AsParallel()
                                       .Select(cmd => target.Procesar(cmd) as ResultadoEjecutar).ToList();

            Assert.That(resultados.Count, Is.EqualTo(10));

            foreach (var resultado in resultados)
            {
                Assert.That(resultado, Is.Not.Null);
                Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
                Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            }

            Thread.Sleep(20);
            Assert.That(dispositivoLiberado, Is.EqualTo("BALDM01"));
            Assert.That(liberaciones, Is.EqualTo(1));
        }

        [Test]
        public void TestProcesadorFinalizado()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    Thread.Sleep(20);
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            target = new ProcesadorDispositivoFisico(dispositivo, 
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp => { }, new NullLogger());
            target.Procesar(new EjecutarCereoCabezal { CodigoDispositivo = "BALDM01" });
            Thread.Sleep(10);
            target.ProcesarRemanentes();

            var resultado = target.Procesar(new EjecutarCereoCabezal {CodigoDispositivo = "BALDM01"}) as ResultadoEjecutar;

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
        }

        [Test]
        public void TestErrorEnDriverFactory()
        {
            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            driverFactoryMock.Setup(f => f.Driver<IDriver>(dispositivo))
                             .Throws(new TipoDispositivoIncorrectoException("Error", dispositivo.Codigo, dispositivo.Configuracion.GetType().Name));

            Assert.That(() => new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp => { }, new NullLogger()), Throws.InstanceOf<TipoDispositivoIncorrectoException>());
        }

        //[Test]
        //public void TestErrorDispositivoInexistente()
        //{
        //    dispositivo = new Dispositivo
        //    {
        //        Codigo = "BALDM01",
        //        Configuracion = new ConfigCabezal(),
        //        Activo = true
        //    };

        //    Assert.That(() => new ProcesadorDispositivoFisico(dispositivo,
        //        procesadorFactoryMock.Object,
        //        driverFactoryMock.Object,
        //        adminSuscripcionesMock.Object,
        //        disp => { }, new NullLogger()), Throws.InstanceOf<DispositivoNoEncontradoException>());
        //}

        //[Test]
        //public void TestErrorDispositivoInactivo()
        //{
        //    dispositivo = new Dispositivo
        //    {
        //        Codigo = "BALDM01",
        //        Configuracion = new ConfigCabezal(),
        //        Activo = false
        //    };

        //    Assert.That(() => new ProcesadorDispositivoFisico(dispositivo,
        //        procesadorFactoryMock.Object,
        //        driverFactoryMock.Object,
        //        adminSuscripcionesMock.Object,
        //        disp => { }, new NullLogger()), Throws.InstanceOf<DispositivoNoEncontradoException>());
        //}

        [Test]
        public void TestProcesarConSuscripcionesActivasNoLiberaDispositivo()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(22)).Returns(true);

            dispositivo = new Dispositivo
            {
                Id = 22,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            var eventoFin = new AutoResetEvent(false);

            bool dispositivoLiberado = false;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());


            var resultado = target.Procesar(new EjecutarPesaje { CodigoDispositivo = "BALDM01" }) as ResultadoEjecutar;

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.False);
        }

        [Test]
        public void TestProcesarLiberandoSuscripcionLiberadispositivo()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(22)).Returns(false);
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(22)).Returns(true);

            dispositivo = new Dispositivo
            {
                Id = 22,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            var eventoFin = new AutoResetEvent(false);

            bool dispositivoLiberado = false;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());


            var resultado = target.Procesar(new EjecutarPesaje { CodigoDispositivo = "BALDM01" }) as ResultadoEjecutar;

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.True);
        }

        [Test]
        public void TestProcesarCreandoSuscripcionMantieneDispositivo()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(22)).Returns(true);
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(22)).Returns(false);

            dispositivo = new Dispositivo
            {
                Id = 22,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            var eventoFin = new AutoResetEvent(false);

            bool dispositivoLiberado = false;
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());


            var resultado = target.Procesar(new EjecutarPesaje { CodigoDispositivo = "BALDM01" }) as ResultadoEjecutar;

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.False);
        }

        [Test]
        public void TestDriverLanzaEvento()
        {
            dispositivo = new Dispositivo
            {
                Id = 22,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };
            
            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp => { }, 
                new NullLogger());

            var notificacion = new NotificacionEvento
                {
                    CodigoDispositivo = "BALDM01",
                    CodigoEvento = CodigosEventos.HumedadRecibida,
                    Valores = new Dictionary<string, decimal> {{"AnalisisHumedad", 12.6M}}
                };

            driverMock.Raise(d=> d.EventoDriver += null, new EventoDriverEventArgs {Notificacion = notificacion});

            adminSuscripcionesMock.Verify(adm => adm.Notificar(notificacion), Times.Once());
        }

        [Test]
        public void TestProcesarEnProcesadorDisposedDaError()
        {
            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true
            };

            target = new ProcesadorDispositivoFisico(dispositivo,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                disp => {}, new NullLogger());

            target.Dispose();

            Assert.That(() => target.Procesar(new EjecutarPesaje {CodigoDispositivo = "BALDM01"}),
                        Throws.InstanceOf<ProcesadorException>());
        }
    }
}
