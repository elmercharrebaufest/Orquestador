using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Web.Mvc;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Unit Test"), TestFixture]
    public class PruebaItcControllerTest
    {

        private PruebaItcController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private IConversor conversor;
        private Mock<IServicioOrquestador> servicioMock;

        private List<ConfigItc> itces;
        private List<Dispositivo> dispositivos;
        private ConfigBarrera configBarrera;
        private ConfigSensor configSensor;
        private ConfigLectorTarjetas configLector;
        private ConfigLectorQr configLectorQr;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            conversor = FactoryConversor.ConversorAutoMapper;
            //repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
            //    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile())
            //    .ToList());
            target = new PruebaItcController(repositorioFactoryMock.Object, servicioMock.Object, new NullLogger());

            itces = new List<ConfigItc>
            {
                new ConfigItc
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        CarInicioFrase = "|",
                        DireccionIp = "host",
                        LongFrase = 5,
                        Puerto = 10,
                        TimeoutLectura = 10,
                        CarFinFrase = " ",
                        ComandoActivarSalida = "3",
                        IntervaloPolling = 2,
                        ComandoEstado = "3",
                        ComandoTarjeta = "T",
                        DelimitadorCampos = ",",
                        RespuestaError = "E",
                        RespuestaExito = "X",
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigItc{Dispositivo = new Dispositivo()}
            };
            itces[0].Dispositivo.Configuracion = itces[0];

            dispositivos = new List<Dispositivo>
                {
                    new Dispositivo{Codigo = "1", Descripcion = "11", EsConcentrador = true, Id = 1},
                    new Dispositivo{Codigo = "2", Descripcion = "22", Id = 2}
                };

            configBarrera = new ConfigBarrera
                {
                    Id = 2, Dispositivo = new Dispositivo {Codigo = "BAR1"}, NumeroSalida = 1
                };
            configSensor = new ConfigSensor
                {
                    Id = 2, Dispositivo = new Dispositivo { Codigo = "SEN1" }, NumeroEntrada = 1,
                };
            configLector = new ConfigLectorTarjetas
                {
                    Id = 2, Dispositivo = new Dispositivo { Codigo = "LEC1" }, Lector = "1",
                };
            configLectorQr = new ConfigLectorQr
            {
                Id = 2,
                Dispositivo = new Dispositivo { Codigo = "LEC1" },
                Lector = "1",
            };
        }


        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(1)).Returns(itces[0].Dispositivo);
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigBarrera, bool>>>(), It.IsAny<Expression<Func<ConfigBarrera, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configBarrera.Dispositivo.Codigo, Numero = configBarrera.NumeroSalida.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(), It.IsAny<Expression<Func<ConfigSensor, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configSensor.Dispositivo.Codigo, Numero = configSensor.NumeroEntrada.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigLectorTarjetas, bool>>>(), It.IsAny<Expression<Func<ConfigLectorTarjetas, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configLector.Dispositivo.Codigo, Numero = configLector.Lector.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigLectorQr, bool>>>(), It.IsAny<Expression<Func<ConfigLectorQr, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configLectorQr.Dispositivo.Codigo, Numero = configLectorQr.Lector.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigDisplay, bool>>>(), It.IsAny<Expression<Func<ConfigDisplay, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configLectorQr.Dispositivo.Codigo, Numero = configLectorQr.Lector.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigCortinaAgua, bool>>>(), It.IsAny<Expression<Func<ConfigCortinaAgua, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel> { new PruebaDispositivoModel { Codigo = configLectorQr.Dispositivo.Codigo, Numero = configLectorQr.Lector.ToString() } });
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigTag, bool>>>(), It.IsAny<Expression<Func<ConfigTag, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel>());
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<ConfigComunicador, bool>>>(), It.IsAny<Expression<Func<ConfigComunicador, PruebaDispositivoModel>>>())).Returns(new List<PruebaDispositivoModel>());

            servicioMock.Setup(s => s.Suscribir(It.IsAny<ComandoSuscribir>())).Returns(new ResultadoSuscribir
                {
                    IdSuscripcion = 1, 
                    Mensaje = new Mensaje(0, "OK")
                });

            var result = target.Index(1) as ViewResult;
            Assert.That(result, Is.Not.Null);
            var model = result.Model as PruebaItcModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.CodigoItc, Is.EqualTo("Cod1"));
            Assert.That(model.Lectores[0].Codigo, Is.EqualTo("LEC1"));
            Assert.That(model.Sensores[0].Codigo, Is.EqualTo("SEN1"));
            Assert.That(model.Barreras[0].Codigo, Is.EqualTo("BAR1"));
            Assert.That(target.ViewBag.Errores, Is.Empty);
        }

        [Test]
        public void TestActivarSalida()
        {
            servicioMock.Setup(x => x.Ejecutar(It.Is<EjecutarAperturaBarrera>(c => c.CodigoDispositivo == "BAR1")))
                        .Returns(new ResultadoEjecutar {Mensaje = new Mensaje(0, "ok")});

            var resultado = target.ActivarSalida("BAR1") as JsonResult;
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Property("Codigo").EqualTo(0));
            Assert.That(resultado.Data, Has.Property("Mensaje").EqualTo("BAR1: 0-ok"));
        }

        [Test]
        public void TestActivarSalidaException()
        {
            servicioMock.Setup(x => x.Ejecutar(It.Is<EjecutarAperturaBarrera>(c => c.CodigoDispositivo == "BAR1")))
                        .Throws<Exception>();

            var resultado = target.ActivarSalida("BAR1") as JsonResult;
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Property("Codigo").EqualTo(999));
        }

        [Test]
        public void TestDesuscribir()
        {
            servicioMock.Setup(x => x.CancelarSuscripcion(It.IsAny<ComandoCancelarSuscripcion>()))
                        .Returns(new ResultadoCancelarSuscripcion {Mensaje = new Mensaje(0, "ok")});
            repositorioMock.Setup(x => x.Listar(It.IsAny<Expression<Func<Suscripcion, bool>>>()))
                           .Returns(new List<Suscripcion>
                               {
                                   new Suscripcion
                                       {
                                           Id = 1,
                                           CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                                           Dispositivo = new Dispositivo {Codigo = "LEC1"}
                                       }
                               });

            var resultado = target.Desuscribir("Cod1") as JsonResult;
            Assert.That(resultado.Data, Is.EqualTo(string.Empty));

        }

    }
}
