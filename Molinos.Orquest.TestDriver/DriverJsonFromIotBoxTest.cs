using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

//using Molinos.ControlDeAcceso.Dominio.DTOs;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Moq;
using Newtonsoft.Json;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using static Molinos.Orquest.Dominio.Constantes;

namespace Molinos.Orquest.TestDriver
{ 
    [TestFixture]
    public class DriverJsonFromIotBoxTest
    {   
        private string codigoDispositivo = "FROMDISPOSITIVOOFFLINE";
        private Mock<IDriverItc> driverItcMock;
        private IDriver DriverFisico;
        private ConfigJsonFromIotBox configDriver;
        private Mock<ILogger> logMock;
        private DriverJsonFromIotBox driver;

        [SetUp]
        public void SetUp()
        {
            logMock = new Mock<ILogger>();
            driverItcMock = new Mock<IDriverItc>();
            configDriver = new ConfigJsonFromIotBox
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverJsonFromIotBox, Molinos.Orquest.DriversImpl",
                NumeroEntrada = 0
            };
            driver = new DriverJsonFromIotBox(logMock.Object);
            driver.Inicializar(codigoDispositivo, configDriver);
            driver.DriverFisico = driverItcMock.Object;
        }


        [Test]
        public void TestVerificarDispositivoDesconectado()
        {
            int desconectado = 0;
            driverItcMock.Setup(d => d.VerificarDispositivo())
                .Callback(() => { desconectado ++; });

            driver.VerificarDispositivo();

            driverItcMock.Verify(d => d.VerificarDispositivo(), Times.Once());
            Assert.IsTrue(desconectado == 1);
        }

        [Test]
        public void TestVerificarDispositivoConectado()
        {
            driverItcMock.Setup(d => d.VerificarDispositivo());

            Assert.DoesNotThrow(() => driver.VerificarDispositivo());
            driverItcMock.Verify(d => d.VerificarDispositivo(), Times.Once());
        }

        [Test]
        public void TestTipoDispositivo()
        {
            Assert.AreEqual(typeof(ConfigJsonFromIotBox), driver.TipoDispositivo);
        }

        [Test]
        public void TestOnEventoDriverFisicoJsonValido()
        {
            var notificacionJsonValido = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string> { { "Dato", "{\"TransitoOffline\":\"valor\"}" } }
            };
            var notificacionJsonInvalido = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string> { { "Dato", "json_invalido" } }
            };

            var eventoValido = new EventoDriverEventArgs { Notificacion = notificacionJsonValido };
            var eventoInvalido = new EventoDriverEventArgs { Notificacion = notificacionJsonInvalido };

            // Usar reflection para invocar el método privado OnEventoDriverFisico
            MethodInfo onEventoDriverFisicoMethod = typeof(DriverJsonFromIotBox).GetMethod("OnEventoDriverFisico", BindingFlags.NonPublic | BindingFlags.Instance);
            onEventoDriverFisicoMethod.Invoke(driver, new object[] { this, eventoValido });
            onEventoDriverFisicoMethod.Invoke(driver, new object[] { this, eventoInvalido });

            // Verificar que el evento fue procesado correctamente
            logMock.Verify(log => log.Info(It.Is<string>(s => s.Contains("EsEventoParaDispositivo - Dato:"))), Times.Once());
            logMock.Verify(log => log.Info(It.Is<string>(s => s.Contains("Evento enviado desde el dispositivo "))), Times.Once());
        }

        [Test]
        public void TestDeterminarEvento()
        {
            var notificacionTransitoOffline = new NotificacionEvento
            {
                Datos = new Dictionary<string, string> { { "Dato", "{\"TransitoOffline\":\"valor\"}" } },
                CodigoEvento = CodigosEventos.TransitoOffline
            };
            var notificacionNoTransitoOffline = new NotificacionEvento
            {
                Datos = new Dictionary<string, string> { { "Dato", "{\"OtroEvento\":\"valor\"}" } },
                CodigoEvento = CodigosEventos.TransitoOffline
            };

            //Act
            //Se utiliza reflection para acceder al método privado determinarEvento
            MethodInfo determinarEventoMethod = typeof(DriverJsonFromIotBox).GetMethod("DeterminarEvento", BindingFlags.NonPublic | BindingFlags.Instance);
            EventoDriverEventArgs resultadoTrue = (EventoDriverEventArgs)determinarEventoMethod.Invoke(driver, new object[] { notificacionTransitoOffline });
            EventoDriverEventArgs resultadoFalse = (EventoDriverEventArgs)determinarEventoMethod.Invoke(driver, new object[] { notificacionNoTransitoOffline });

            //Assert
            Assert.AreEqual(CodigosEventos.TransitoOffline, resultadoTrue.Notificacion.CodigoEvento);
            Assert.IsNull(resultadoFalse);
        }

        [Test]
        public void TestEsEventoParaDispositivo()
        {
            var notificacionCorrecta = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string> { { "Entrada", "0" } }
            };
            var notificacionEventoNoSoportado = new NotificacionEvento
            {
                CodigoEvento = "EventoNoSoportado",
                Datos = new Dictionary<string, string> { { "Entrada", "0" } }
            };
            var notificacionEntradaIncorrecta = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string> { { "Entrada", "1" } }
            };
            var notificacionSinEntrada = new NotificacionEvento
            {
                CodigoEvento = CodigosEventos.EntradaActivada,
                Datos = new Dictionary<string, string>()
            };

            Assert.IsTrue(driver.EsEventoParaDispositivo(notificacionCorrecta));
            Assert.IsFalse(driver.EsEventoParaDispositivo(notificacionEventoNoSoportado));
            Assert.IsFalse(driver.EsEventoParaDispositivo(notificacionEntradaIncorrecta));
            Assert.IsTrue(driver.EsEventoParaDispositivo(notificacionSinEntrada));
            Assert.IsTrue(driver.EventosSoportados.Contains(notificacionCorrecta.CodigoEvento));
        }
    }
}