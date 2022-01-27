using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Test.Mocks;
using Molinos.Orquest.Web.Controllers;
using Molinos.Orquest.Web.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Molinos.Orquest.Test.Controllers
{
    [TestFixture]
    public class PruebaMolineteControllerTest
    {
        private PruebaMolineteController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IServicioOrquestador> servicioMock;
        private List<ConfigMolinete> molinete;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);

            target = new PruebaMolineteController(repositorioFactoryMock.Object, servicioMock.Object, new NullLogger());

            molinete = new List<ConfigMolinete>
            {
                new ConfigMolinete
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        DireccionUrl="ww.urltest.cm",
                        IntervaloPooling=10,
                        TimeoutHabilitacion=20,
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }, new ConfigMolinete{Dispositivo = new Dispositivo()}
            };
            molinete[0].Dispositivo.Configuracion = molinete[0];
        }


        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(x => x.Obtener<Dispositivo>(1)).Returns(molinete[0].Dispositivo);

            servicioMock.Setup(s => s.Suscribir(It.IsAny<ComandoSuscribir>())).Returns(new ResultadoSuscribir
            {
                IdSuscripcion = 1,
                Mensaje = new Mensaje(0, "OK")
            });

            var result = target.Index(1) as ViewResult;
            Assert.That(result, Is.Not.Null);
            Assert.NotNull(result.ViewBag.Errores.Count);
            Assert.AreEqual(result.ViewBag.Errores.Count, 0);
            Assert.That(target.ViewBag.Errores, Is.Empty);
            Assert.AreEqual(result.ViewBag.IdMolinete, 1);
            var model = result.Model as PruebaMolineteModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.CodigoMolinete, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestHabilitarTransito()
        {
            servicioMock.Setup(x => x.Ejecutar(It.Is<EjecutarHabilitarTransito>(c => c.CodigoDispositivo == "BAR1")))
                        .Returns(new ResultadoEjecutar { Mensaje = new Mensaje(0, "ok") });

            var resultado = target.HabilitarTransito("BAR1", "Entrada") as JsonResult;
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Data, Has.Property("Codigo").EqualTo(0));
            Assert.That(resultado.Data, Has.Property("Mensaje").EqualTo("BAR1: 0-ok"));
        }

        [Test]
        public void TestDesuscribir()
        {
            servicioMock.Setup(x => x.CancelarSuscripcion(It.IsAny<ComandoCancelarSuscripcion>()))
                        .Returns(new ResultadoCancelarSuscripcion { Mensaje = new Mensaje(0, "ok") });
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
