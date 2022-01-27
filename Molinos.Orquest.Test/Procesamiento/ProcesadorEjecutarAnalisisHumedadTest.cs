using System;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Procesamiento
{
    class ProcesadorEjecutarAnalisisHumedadTest
    {
        private ProcesadorEjecutarAnalisisHumedad target;

        [SetUp]
        public void SetUp()
        {
            target = new ProcesadorEjecutarAnalisisHumedad(new NullLogger());
        }

        [Test]
        public void TestProcesarOK()
        {
            var comando = new EjecutarAnalisisHumedad { CodigoDispositivo = "BAL01", FechaDeInicio = DateTime.Now };
            var dispositivo = new Dispositivo
            {
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverHumedimetro>();
            driverMock.Setup(d => d.ObtenerHumedad(It.IsAny<DateTime>())).Returns(123);

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            Assert.That(resultadoEjecutar.Valores, Is.Not.Null.And.Count.EqualTo(1));
            Assert.That(resultadoEjecutar.Valores["AnalisisHumedad"], Is.EqualTo(123));
        }

        [Test]
        public void TestProcesarErrorEnDriverFormatoRespuesta()
        {
            var comando = new EjecutarAnalisisHumedad { CodigoDispositivo = "HUM01", FechaDeInicio = DateTime.Now };
            var dispositivo = new Dispositivo
            {
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverHumedimetro>();
            driverMock.Setup(d => d.ObtenerHumedad(It.IsAny<DateTime>())).Throws<FormatoRespuestaDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.FormatoRespuestaDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverConexion()
        {
            var comando = new EjecutarAnalisisHumedad { CodigoDispositivo = "HUM01", FechaDeInicio =  DateTime.Now};
            var dispositivo = new Dispositivo
            {
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverHumedimetro>();
            driverMock.Setup(d => d.ObtenerHumedad(It.IsAny<DateTime>())).Throws<ConexionDispositivoDriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);

            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ConexionDispositivo));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

        [Test]
        public void TestProcesarErrorEnDriverGenerico()
        {
            var comando = new EjecutarAnalisisHumedad { CodigoDispositivo = "HUM01", FechaDeInicio = DateTime.Now };
            var dispositivo = new Dispositivo
            {
                Codigo = "HUM01",
                Configuracion = new ConfigHumedimetro { ClaseDriver = "MiDriver" }
            };

            var driverMock = new Mock<IDriverHumedimetro>();
            driverMock.Setup(d => d.ObtenerHumedad(It.IsAny<DateTime>())).Throws<DriverException>();

            var resultadoEjecutar = target.Procesar(comando, dispositivo, driverMock.Object);
            Assert.That(resultadoEjecutar, Is.Not.Null);
            Assert.That(resultadoEjecutar.Mensaje, Is.Not.Null);

            Assert.That(resultadoEjecutar.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));

            Assert.That(resultadoEjecutar.Valores, Is.Empty);
        }

    }
}
