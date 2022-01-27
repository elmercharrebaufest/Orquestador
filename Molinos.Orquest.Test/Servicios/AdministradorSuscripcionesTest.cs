using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class AdministradorSuscripcionesTest
    {
        private AdministradorSuscripciones target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;

        private Mock<IServicioRemotoFactory> servicioRemotoFactoryMock;
        private Mock<IServicioSuscriptor> suscriptorMock;

        private Suscripcion[] suscripciones;
        private Dispositivo dispositivo1;
        private Dispositivo dispositivo2;

        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            servicioRemotoFactoryMock = new Mock<IServicioRemotoFactory>();

            target = new AdministradorSuscripciones(repositorioFactoryMock.Object, servicioRemotoFactoryMock.Object, new NullLogger());

            suscriptorMock = new Mock<IServicioSuscriptor>();
            servicioRemotoFactoryMock.Setup(f => f.CrearServicioSuscriptor(It.IsAny<string>()))
                                     .Returns(new ClienteServicio<IServicioSuscriptor>(suscriptorMock.Object));

            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Suscripcion, bool>>>()))
                .Returns<Expression<Func<Suscripcion, bool>>>(filtro => suscripciones.SingleOrDefault(filtro.Compile()));

            repositorioMock.Setup(r => r.Existe(It.IsAny<Expression<Func<Suscripcion, bool>>>()))
                .Returns<Expression<Func<Suscripcion, bool>>>(filtro => suscripciones.Any(filtro.Compile()));

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>()))
                           .Returns<Expression<Func<Suscripcion, bool>>>(filtro => suscripciones.Where(filtro.Compile()).ToList());

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>(), It.IsAny<Expression<Func<Suscripcion, string>>>()))
                           .Returns<Expression<Func<Suscripcion, bool>>, Expression<Func<Suscripcion, string>>>((filtro, seleccion) => suscripciones.Where(filtro.Compile()).Select(seleccion.Compile()).ToList());

            dispositivo1 = new Dispositivo
                {
                    Id = 10,
                    Codigo = "HUM01"
                };

            dispositivo2 = new Dispositivo
                {
                    Id = 20,
                    Codigo = "HUM02"
                };

            suscripciones = new[]
                {
                    new Suscripcion
                    {
                        Id = 1,
                        CodigoEvento = CodigosEventos.HumedadRecibida,
                        Dispositivo = dispositivo1,
                        Cantidad = 3,
                        RutaAccesoSuscriptor = "http://suscriptor1",
                        UltimaSuscripcion = DateTime.Now.AddMinutes(-1)
                    },
                    new Suscripcion
                    {
                        Id = 2,
                        CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                        Dispositivo = dispositivo1,
                        Cantidad = 3,
                        RutaAccesoSuscriptor = "http://suscriptor1",
                        UltimaSuscripcion = DateTime.Now.AddMinutes(-2)
                    },
                    new Suscripcion
                    {
                        Id = 3,
                        CodigoEvento = CodigosEventos.HumedadRecibida,
                        Dispositivo = dispositivo1,
                        Cantidad = 3,
                        RutaAccesoSuscriptor = "http://suscriptor2",
                        UltimaSuscripcion = DateTime.Now.AddMinutes(-1)
                    },
                    new Suscripcion
                    {
                        Id = 4,
                        CodigoEvento = CodigosEventos.HumedadRecibida,
                        Dispositivo = dispositivo2,
                        Cantidad = 3,
                        RutaAccesoSuscriptor = "http://suscriptor1",
                        UltimaSuscripcion = DateTime.Now.AddMinutes(-3)
                    }
                };

        }

        [Test]
        public void TestCrearPrimeraSuscripcion()
        {
            var dispositivo = new Dispositivo
                {
                    Id = 10,
                    Codigo = "HUM01"
                };
            Suscripcion suscripcion = null;
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Suscripcion, bool>>>())).Returns((Suscripcion)null);
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Suscripcion>()))
                           .Returns<Suscripcion>(s =>
                           {
                               suscripcion = s;
                               suscripcion.Id = 123;
                               return suscripcion;
                           });
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(10)).Returns(dispositivo);
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", false);

            Assert.That(id, Is.EqualTo(123));
            Assert.That(suscripcion, Is.Not.Null);
            Assert.That(suscripcion.Id, Is.EqualTo(123));
            Assert.That(suscripcion.Dispositivo, Is.EqualTo(dispositivo));
            Assert.That(suscripcion.RutaAccesoSuscriptor, Is.EqualTo("http://suscriptor1"));
            Assert.That(suscripcion.Cantidad, Is.EqualTo(1));
            Assert.That(suscripcion.Persistente, Is.False);
            // Deberiamos verificar que se le puso el timestamp de ese momento, esto es lo más cercano que se puede hacer
            Assert.That(DateTime.Now.Subtract(suscripcion.UltimaSuscripcion), Is.LessThan(TimeSpan.FromSeconds(1)));

            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearPrimeraSuscripcionPersistente()
        {
            var dispositivo = new Dispositivo
            {
                Id = 10,
                Codigo = "HUM01"
            };
            Suscripcion suscripcion = null;
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Suscripcion, bool>>>())).Returns((Suscripcion)null);
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Suscripcion>()))
                           .Returns<Suscripcion>(s =>
                           {
                               suscripcion = s;
                               suscripcion.Id = 123;
                               return suscripcion;
                           });
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(10)).Returns(dispositivo);
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", true);

            Assert.That(id, Is.EqualTo(123));
            Assert.That(suscripcion, Is.Not.Null);
            Assert.That(suscripcion.Id, Is.EqualTo(123));
            Assert.That(suscripcion.Dispositivo, Is.EqualTo(dispositivo));
            Assert.That(suscripcion.RutaAccesoSuscriptor, Is.EqualTo("http://suscriptor1"));
            Assert.That(suscripcion.Cantidad, Is.EqualTo(1));
            Assert.That(suscripcion.Persistente, Is.True);
            // Deberiamos verificar que se le puso el timestamp de ese momento, esto es lo más cercano que se puede hacer
            Assert.That(DateTime.Now.Subtract(suscripcion.UltimaSuscripcion), Is.LessThan(TimeSpan.FromSeconds(1)));

            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearSuscripcionSobreYaExistente()
        {
            var fechaAnterior = suscripciones[0].UltimaSuscripcion;
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", false);

            Assert.That(id, Is.EqualTo(1));
            Assert.That(suscripciones[0].Cantidad, Is.EqualTo(4));
            Assert.That(suscripciones[0].UltimaSuscripcion, Is.GreaterThan(fechaAnterior));
            Assert.That(suscripciones[0].Persistente, Is.False);

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearSuscripcionPersistenteSobreYaExistente()
        {
            var fechaAnterior = suscripciones[0].UltimaSuscripcion;
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", true);

            Assert.That(id, Is.EqualTo(1));
            Assert.That(suscripciones[0].Cantidad, Is.EqualTo(4));
            Assert.That(suscripciones[0].UltimaSuscripcion, Is.GreaterThan(fechaAnterior));
            Assert.That(suscripciones[0].Persistente, Is.True);

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearSuscripcionNoPersistenteSobreYaExistente()
        {
            var fechaAnterior = suscripciones[0].UltimaSuscripcion;
            suscripciones[0].Persistente = true;
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", false);

            Assert.That(id, Is.EqualTo(1));
            Assert.That(suscripciones[0].Cantidad, Is.EqualTo(4));
            Assert.That(suscripciones[0].UltimaSuscripcion, Is.GreaterThan(fechaAnterior));
            Assert.That(suscripciones[0].Persistente, Is.True);

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCrearSuscripcionSobreYaExistentePersistente()
        {
            var fechaAnterior = suscripciones[0].UltimaSuscripcion;
            var id = target.CrearSuscripcion(10, CodigosEventos.HumedadRecibida, "http://suscriptor1", true);

            Assert.That(id, Is.EqualTo(1));
            Assert.That(suscripciones[0].Cantidad, Is.EqualTo(4));
            Assert.That(suscripciones[0].UltimaSuscripcion, Is.GreaterThan(fechaAnterior));
            Assert.That(suscripciones[0].Persistente, Is.True);

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCancelarUltimaSuscripcionParaEseSuscriptor()
        {
            suscripciones[0].Cantidad = 1;

            target.CancelarSuscripcion(1, 10, false);

            repositorioMock.Verify(r => r.Remover(suscripciones[0]), Times.Once());
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCancelarSuscripcionParaEseSuscriptorConMasPendientes()
        {
            var fechaAntrerior = suscripciones[0].UltimaSuscripcion;
            suscripciones[0].Cantidad = 2;

            target.CancelarSuscripcion(1, 10, false);

            Assert.That(suscripciones[0].Cantidad, Is.EqualTo(1));
            Assert.That(suscripciones[0].UltimaSuscripcion, Is.EqualTo(fechaAntrerior));
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCancelarTodasSuscripcionesPendientesDeUnEventoParaUnSuscriptor()
        {
            var fechaAntrerior = suscripciones[0].UltimaSuscripcion;
            suscripciones[0].Cantidad = 2;

            target.CancelarSuscripcion(1, 10, true);

            repositorioMock.Verify(r => r.Remover(suscripciones[0]), Times.Once());
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestCancelarSuscripcionDeOtroDispositivoAlEspecificado()
        {
            suscripciones[0].Cantidad = 2;

            Assert.That(() => target.CancelarSuscripcion(1, 20, false), Throws.InstanceOf<SuscripcionNoEncontradaException>());

            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Never());
        }

        [Test]
        public void TestExistenSuscripcionesParaDispositivo()
        {
            Assert.That(target.ExistenSuscripcionesPara(10), Is.True);
            Assert.That(target.ExistenSuscripcionesPara(20), Is.True);
            Assert.That(target.ExistenSuscripcionesPara(30), Is.False);
        }

        [Test]
        public void TestNotificarOKConUnSuscriptor()
        {
            var notificacion = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                CodigoDispositivo = "HUM01",
                Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.5M } }
            };

            target.Notificar(notificacion);
            Thread.Sleep(1000);
            servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor1"), Times.Once());
            servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor(It.IsAny<string>()), Times.Exactly(1));

            suscriptorMock.Verify(s => s.Recibir(notificacion), Times.Exactly(1));
        }

        //[Test]
        //public async void TestNotificarOKConDosSuscriptores()
        //{
        //    var notificacion = new NotificacionEvento
        //        {
        //            CodigoEvento = CodigosEventos.HumedadRecibida,
        //            CodigoDispositivo = "HUM01",
        //            Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.5M } }
        //        };

        //    target.Notificar(notificacion);
        //    Thread.Sleep(1000);
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor1"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor2"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor(It.IsAny<string>()), Times.Exactly(2));

        //    suscriptorMock.Verify(s => s.Recibir(notificacion), Times.Exactly(2));
        //}

        [Test]
        public void TestNotificarOKConDosSuscriptoresUnaDaError()
        {
            // Si ocurre un error al entrega una notificación al suscriptor, igualmente se debe seguir con las demas.
            // NO se debe abortar la operacion, solo loguear el error
            var notificacion = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.HumedadRecibida,
                CodigoDispositivo = "HUM01",
                Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.5M } }
            };

            var suscriptorConErrorMock = new Mock<IServicioSuscriptor>();
            suscriptorConErrorMock.Setup(s => s.Recibir(notificacion)).Throws<Exception>();

            servicioRemotoFactoryMock.Setup(s => s.CrearServicioSuscriptor("http://suscriptor1"))
                          .Returns(new ClienteServicio<IServicioSuscriptor>(suscriptorConErrorMock.Object));

            target.Notificar(notificacion);
            Thread.Sleep(2000);

            servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor1"), Times.Once());
            servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor2"), Times.Once());
            servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor(It.IsAny<string>()), Times.Exactly(2));

            suscriptorConErrorMock.Verify(s => s.Recibir(notificacion), Times.Once());
            suscriptorMock.Verify(s => s.Recibir(notificacion), Times.Once());
        }

        //[Test]
        //public void TestNotificarOKCon10Suscriptores()
        //{
        //    suscripciones = new Suscripcion[10];
        //    for (int i = 0; i < 10; i++)
        //    {
        //        suscripciones[i] = new Suscripcion
        //        {
        //            Id = i,
        //            CodigoEvento = CodigosEventos.HumedadRecibida,
        //            Dispositivo = dispositivo1,
        //            Cantidad = 3,
        //            RutaAccesoSuscriptor = "http://suscriptor" + i,
        //            UltimaSuscripcion = DateTime.Now.AddMinutes(-1)
        //        };
        //    }
        //    suscriptorMock.Setup(s => s.Recibir(It.IsAny<NotificacionEvento>())).Callback(() => Thread.Sleep(1000));

        //    var notificacion = new NotificacionEvento
        //    {
        //        CodigoEvento = CodigosEventos.HumedadRecibida,
        //        CodigoDispositivo = "HUM01",
        //        Valores = new Dictionary<string, decimal> { { "AnalisisHumedad", 12.5M } }
        //    };
        //    var stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    target.Notificar(notificacion);
        //    stopwatch.Stop();

        //    Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10000));

        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor0"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor1"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor2"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor3"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor4"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor5"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor6"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor7"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor8"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor("http://suscriptor9"), Times.Once());
        //    servicioRemotoFactoryMock.Verify(s => s.CrearServicioSuscriptor(It.IsAny<string>()), Times.Exactly(10));

        //    suscriptorMock.Verify(s => s.Recibir(notificacion), Times.Exactly(10));
        //}

        [Test]
        public void TestDepurarSuscripciones()
        {
            var fecha = DateTime.Now;

            suscripciones[0].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[1].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[2].UltimaSuscripcion = fecha.AddHours(-1);
            suscripciones[3].UltimaSuscripcion = fecha.AddDays(-1); //Este es otro dispositivo, no debería eliminarla

            target.DepurarSuscripciones(10, fecha.AddHours(-2), false);

            repositorioMock.Verify(r => r.Remover(suscripciones[0]), Times.Once());
            repositorioMock.Verify(r => r.Remover(suscripciones[1]), Times.Once());
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Exactly(2));
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestDepurarSuscripcionesPersistentes()
        {
            var fecha = DateTime.Now;

            suscripciones[0].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[1].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[2].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[2].Persistente = true;
            suscripciones[3].UltimaSuscripcion = fecha.AddDays(-1); //Este es otro dispositivo, no debería eliminarla

            target.DepurarSuscripciones(10, fecha.AddHours(-2), true);

            repositorioMock.Verify(r => r.Remover(suscripciones[0]), Times.Once());
            repositorioMock.Verify(r => r.Remover(suscripciones[1]), Times.Once());
            repositorioMock.Verify(r => r.Remover(suscripciones[2]), Times.Once());
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Exactly(3));
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void TestDepurarSuscripcionesNoPersistentes()
        {
            var fecha = DateTime.Now;

            suscripciones[0].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[1].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[2].UltimaSuscripcion = fecha.AddDays(-1);
            suscripciones[2].Persistente = true; //Esta es persistente, no debería eliminarla
            suscripciones[3].UltimaSuscripcion = fecha.AddDays(-1); //Este es otro dispositivo, no debería eliminarla

            target.DepurarSuscripciones(10, fecha.AddHours(-2), false);

            repositorioMock.Verify(r => r.Remover(suscripciones[0]), Times.Once());
            repositorioMock.Verify(r => r.Remover(suscripciones[1]), Times.Once());
            repositorioMock.Verify(r => r.Remover(It.IsAny<Suscripcion>()), Times.Exactly(2));
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

    }
}
