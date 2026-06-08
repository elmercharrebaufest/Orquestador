using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public class ProcesadorDispositivoConcentradorTest
    {
        private ProcesadorDispositivoConcentrador target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IProcesadorFactory> procesadorFactoryMock;
        private Mock<IProcesadorComando> procesadorMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private Mock<IDriver> driverMock;
        private Mock<IDriver> driverLogicoMock;
        private Mock<IAdministradorSuscripciones> adminSuscripcionesMock;
        private Mock<IAdministradorIdentificacionVehicular> adminIdentificacionMock;

        private Dispositivo dispositivo;
        private Dispositivo dispositivo2;
        private Dispositivo concentrador;

        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioMock = new Mock<IRepositorio>();
            procesadorFactoryMock = new Mock<IProcesadorFactory>();
            procesadorMock = new Mock<IProcesadorComando>();
            driverFactoryMock = new Mock<IDriverFactory>();
            driverMock = new Mock<IDriver>();
            driverLogicoMock = new Mock<IDriver>();
            adminSuscripcionesMock = new Mock<IAdministradorSuscripciones>();
            adminIdentificacionMock = new Mock<IAdministradorIdentificacionVehicular>();

            procesadorFactoryMock.Setup(factory => factory.ProcesadorPara(It.IsAny<Comando>())).Returns(procesadorMock.Object);
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                   .Returns<Expression<Func<Dispositivo, bool>>>(
                       condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
        }

        [Test]
        public void TestProcesarComandoConColaVacia()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            concentrador = new Dispositivo
                {
                    Codigo = "ITCDM01",
                    Configuracion = new ConfigItc(),
                    Activo = true,
                    EsConcentrador = true
                };

            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };

            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo>{dispositivo});

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
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
        public void TestProcesarDispositivoIncorrecto()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            concentrador = new Dispositivo
            {
                Codigo = "ITCDM01",
                Configuracion = new ConfigItc(),
                Activo = true,
                EsConcentrador = true
            };

            dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };

            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo> { dispositivo });

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());

            Assert.That(() => target.Procesar(new EjecutarCereoCabezal { CodigoDispositivo = "BALDM99" }),
            Throws.InstanceOf<ProcesadorException>());

            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.False);
        }

        [Test]
        public void TestProcesarConSuscripcionesActivas()
        {
            procesadorMock.Setup(proc => proc.Procesar(It.IsAny<Comando>(), It.IsAny<Dispositivo>(), It.IsAny<IDriver>()))
                .Returns<Comando, Dispositivo, IDriver>((cmd, distp, driver) =>
                {
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            concentrador = new Dispositivo
            {
                
                Id = 10,
                Codigo = "ITCDM01",
                Configuracion = new ConfigItc(),
                Activo = true,
                EsConcentrador = true
            };

            dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };
            dispositivo2 = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM02",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };

            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM02"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo> { dispositivo, dispositivo2 });

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(2)).Returns(true);

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
                    adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(2)).Returns(false);
                    return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                });

            concentrador = new Dispositivo
            {

                Id = 10,
                Codigo = "ITCDM01",
                Configuracion = new ConfigItc(),
                Activo = true,
                EsConcentrador = true
            };

            dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };
            dispositivo2 = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM02",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };

            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM02"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo> { dispositivo, dispositivo2 });

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());

            adminSuscripcionesMock.Setup(adm => adm.ExistenSuscripcionesPara(2)).Returns(true);

            var resultado = target.Procesar(new EjecutarPesaje { CodigoDispositivo = "BALDM01" }) as ResultadoEjecutar;

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(0));
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500));
            eventoFin.WaitOne();
            Assert.That(dispositivoLiberado, Is.False);
        }

        [Test]
        public void TestDriverLogicoLanzaEvento()
        {
            concentrador = new Dispositivo
            {

                Id = 10,
                Codigo = "ITCDM01",
                Configuracion = new ConfigItc(),
                Activo = true,
                EsConcentrador = true
            };

            dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };


            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo> { dispositivo });

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "BALDM01",
                CodigoEvento = CodigosEventos.HumedadRecibida,
                Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.6M } }
            };

            driverLogicoMock.Raise(d => d.EventoDriver += null, new EventoDriverEventArgs { Notificacion = notificacion });

            adminSuscripcionesMock.Verify(adm => adm.Notificar(notificacion), Times.Once());
        }

        [Test]
        public void TestDriverFisicoLanzaEvento()
        {
            concentrador = new Dispositivo
            {

                Id = 10,
                Codigo = "ITCDM01",
                Configuracion = new ConfigItc(),
                Activo = true,
                EsConcentrador = true
            };

            dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Configuracion = new ConfigCabezal(),
                Activo = true,
                Concentrador = concentrador
            };


            driverFactoryMock.Setup(f => f.Driver<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "ITCDM01"))).Returns(driverMock.Object);
            driverFactoryMock.Setup(f => f.DriverLogico<IDriver>(It.Is<Dispositivo>(d => d.Codigo == "BALDM01"), driverMock.Object)).Returns(driverLogicoMock.Object);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(new List<Dispositivo> { dispositivo });

            var eventoFin = new AutoResetEvent(false);

            var dispositivoLiberado = false;
            target = new ProcesadorDispositivoConcentrador(concentrador,
                repositorioFactoryMock.Object,
                procesadorFactoryMock.Object,
                driverFactoryMock.Object,
                adminSuscripcionesMock.Object,
                adminIdentificacionMock.Object,
                disp =>
                {
                    dispositivoLiberado = disp.ProcesarRemanentes();
                    eventoFin.Set();
                }, new NullLogger());

            var notificacion = new NotificacionEvento
            {
                CodigoDispositivo = "BALDM01",
                CodigoEvento = CodigosEventos.HumedadRecibida,
                Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.6M } }
            };

            driverMock.Raise(d => d.EventoDriver += null, new EventoDriverEventArgs { Notificacion = notificacion });
            // No debe haber suscripciones a drivers fisicos
            adminSuscripcionesMock.Verify(adm => adm.Notificar(It.IsAny<NotificacionEvento>()), Times.Never());
        }
    }
}
