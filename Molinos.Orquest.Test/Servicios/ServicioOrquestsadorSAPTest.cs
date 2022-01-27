using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class ServicioOrquestsadorSAPTest
    {
        private ServicioOrquestadorSAP target;
        private Mock<IServicioOrquestador> orquestadorMock;

        [SetUp]
        public void SetUp()
        {
            orquestadorMock = new Mock<IServicioOrquestador>();
            target = new ServicioOrquestadorSAP(orquestadorMock.Object, new NullLogger());
        }

        [Test]
        public void TestEjecutarPesajeOK()
        {
            var resultadoComando = new ResultadoEjecutar {Mensaje = Mensaje.ResultadoOK()}.Agregar("Pesaje", 123);

            orquestadorMock.Setup(orq => orq.Ejecutar(It.Is<EjecutarPesaje>(x => x.CodigoDispositivo == "BAL")))
                           .Returns(resultadoComando);

            var resultado = target.EjecutarPesaje("BAL");
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Codigo, Is.EqualTo(resultadoComando.Mensaje.Codigo));
            Assert.That(resultado.Descripcion, Is.EqualTo(resultadoComando.Mensaje.Descripcion));
            Assert.That(resultado.Valor, Is.EqualTo(resultadoComando.Valores["Pesaje"]));
        }

        [Test]
        public void TestEjecutarPesajeError()
        {
            var resultadoComando = new ResultadoEjecutar { Mensaje = new Mensaje(123, "Esto es un error") };

            orquestadorMock.Setup(orq => orq.Ejecutar(It.Is<EjecutarPesaje>(x => x.CodigoDispositivo == "BAL")))
                           .Returns(resultadoComando);

            var resultado = target.EjecutarPesaje("BAL");
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Codigo, Is.EqualTo(resultadoComando.Mensaje.Codigo));
            Assert.That(resultado.Descripcion, Is.EqualTo(resultadoComando.Mensaje.Descripcion));
            Assert.That(resultado.Valor, Is.EqualTo(0));
        }

        [Test]
        public void TestEjecutarCereoCabezalOK()
        {
            var resultadoComando = new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() };

            orquestadorMock.Setup(orq => orq.Ejecutar(It.Is<EjecutarCereoCabezal>(x => x.CodigoDispositivo == "BAL")))
                           .Returns(resultadoComando);

            var resultado = target.EjecutarCereoCabezal("BAL");
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Codigo, Is.EqualTo(resultadoComando.Mensaje.Codigo));
            Assert.That(resultado.Descripcion, Is.EqualTo(resultadoComando.Mensaje.Descripcion));
            Assert.That(resultado.Valor, Is.EqualTo(0));
        }

        [Test]
        public void TestEjecutarCereoCabezalError()
        {
            var resultadoComando = new ResultadoEjecutar { Mensaje = new Mensaje(123, "Esto es un error") };

            orquestadorMock.Setup(orq => orq.Ejecutar(It.Is<EjecutarCereoCabezal>(x => x.CodigoDispositivo == "BAL")))
                           .Returns(resultadoComando);

            var resultado = target.EjecutarCereoCabezal("BAL");
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Codigo, Is.EqualTo(resultadoComando.Mensaje.Codigo));
            Assert.That(resultado.Descripcion, Is.EqualTo(resultadoComando.Mensaje.Descripcion));
            Assert.That(resultado.Valor, Is.EqualTo(0));
        }
    }
}
