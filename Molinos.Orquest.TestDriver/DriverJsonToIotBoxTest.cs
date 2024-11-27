using System;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using Moq;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    public class DriverJsonToIotBoxTest
    {
        private DriverJsonToIotBox driver;
        private Mock<IDriverItc> driverItcMock;
        private Mock<ILogger> logMock;

        [SetUp]
        public void SetUp()
        {
            logMock = new Mock<ILogger>();
            driverItcMock = new Mock<IDriverItc>();
            driver = new DriverJsonToIotBox(logMock.Object)
            {
                DriverFisico = driverItcMock.Object
            };
        }

        [Test]
        public void TestInicializar()
        {
            var config = new ConfigJsonToIotBox();
            driver.Inicializar("codigo", config);
            logMock.Verify(log => log.Info(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestEnviarJsonConJsonValido()
        {
            string jsonValido = "{\"nombre\":\"valor\"}";
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());

            Assert.DoesNotThrow(() => driver.EnviarJson(jsonValido));
            driverItcMock.Verify(d => d.ActivarSalida(It.IsAny<int>(), jsonValido, It.IsAny<string>(),It.IsAny<bool>()), Times.Once());
            logMock.Verify(log => log.Info(It.Is<string>(s => s.Contains("Json: "))), Times.Once());
        }

        [Test]
        public void TestEnviarJsonConJsonInvalido()
        {
            string jsonInvalido = "esto no es un json";
            Assert.Throws<DriverConMensajeException>(() => driver.EnviarJson(jsonInvalido));
        }

        [Test]
        public void TestEnviarJsonConReintento()
        {
            var count = 0;
            string jsonValido = "{\"nombre\":\"valor\"}";
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());

            driverItcMock.Setup(d => d.ActivarSalida(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback(() => {
                    count++; 
                    if (count == 1)
                        throw new DriverConMensajeException();
                });

            Assert.Throws(typeof(DriverConMensajeException), () => driverItcMock.Object.ActivarSalida(It.IsAny<int>(), jsonValido, It.IsAny<string>(), It.IsAny<bool>()));
            Assert.DoesNotThrow(() => driverItcMock.Object.ActivarSalida(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()));

        }

        [Test]
        public void TestEnviarJsonConReintentoExitoso()
        {
            string jsonValido = "{\"nombre\":\"valor\"}";
            var count = 0;
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());
            driverItcMock.Setup(d => d.ActivarSalida(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Callback(() => {
                    count++;
                    if (count == 1)
                        throw new DriverConMensajeException();
                });

            Assert.DoesNotThrow(() => driver.EnviarJson(jsonValido));
            logMock.Verify(log => log.Info(It.Is<string>(s => s.Contains("Json: "))), Times.Once());
       }

        [Test]
        public void TestEnviarJsonConReintentoFallido()
        {
            string jsonValido = "{\"nombre\":\"valor\"}";
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());
            driverItcMock.Setup(d => d.ActivarSalida(It.IsAny<int>(), jsonValido, It.IsAny<string>(), It.IsAny<bool>()))
                         .Throws(new Exception());

            Assert.Throws<DriverConMensajeException>(() => driver.EnviarJson(jsonValido));
            driverItcMock.Verify(d => d.ActivarSalida(It.IsAny<int>(), jsonValido, It.IsAny<string>(), It.IsAny<bool>()), Times.Exactly(2));

        }

        [Test]
        public void TestVerificarDispositivoDesconectado()
        {
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());
            driverItcMock.Setup(d => d.VerificarDispositivo())
                .Callback(()=> { throw new ConexionDispositivoDriverException(); });

            Assert.Throws<ConexionDispositivoDriverException>(() => driver.VerificarDispositivo());
            driverItcMock.Verify(d => d.VerificarDispositivo(), Times.Once());
        }

        [Test]
        public void TestVerificarDispositivoConectado()
        {
            driver.Inicializar(It.IsAny<string>(), new ConfigJsonToIotBox());
            driverItcMock.Setup(d => d.VerificarDispositivo());

            Assert.DoesNotThrow(() => driver.VerificarDispositivo());
            driverItcMock.Verify(d => d.VerificarDispositivo(), Times.Once());
        }

        [Test]
        public void TestTipoDispositivo()
        {
            Assert.AreEqual(typeof(ConfigJsonToIotBox), driver.TipoDispositivo);
        }
    }
}
