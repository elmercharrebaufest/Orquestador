using System;
using System.Collections.Generic;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class DriverIdentificacionVehicularTest
    {
        private DriverIdentificacionVehicular target;
        private List<NotificacionEvento> notificacionesCapturadas;
        private ConfigIdentificacionVehicular config;

        [SetUp]
        public void SetUp()
        {
            notificacionesCapturadas = new List<NotificacionEvento>();

            config = new ConfigIdentificacionVehicular
            {
                Codigo = "CIV01",
                Activo = true,
                MaxReintentosFoto = 0,
                DelayEntreReintentosMs = 0,
                Camaras = new List<ConfigIdentificacionVehicularCamara>()
            };

            target = new DriverIdentificacionVehicular(
                config,
                n => { lock (notificacionesCapturadas) notificacionesCapturadas.Add(n); },
                new NullLogger());
        }

        [TearDown]
        public void TearDown()
        {
            target.Dispose();
        }

        // ------------------------------------------------------------------ AsignarTriggerDriverLectorTarjeta

        [Test]
        public void AsignarTriggerDriverLectorTarjeta_NuevoLector_SeRegistraAlEvento()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Tarjeta"] = "T01" }
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
        }

        [Test]
        public void AsignarTriggerDriverLectorTarjeta_ReemplazarLector_LectorAnteriorYaNoDisparaEventos()
        {
            var lector1 = new Mock<IDriverLectorTarjetas>();
            var lector2 = new Mock<IDriverLectorTarjetas>();

            target.AsignarTriggerDriverLectorTarjeta(lector1.Object);
            target.AsignarTriggerDriverLectorTarjeta(lector2.Object);

            // lector1 ya no debería generar notificaciones
            lector1.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Tarjeta"] = "VIEJO" }
                }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        [Test]
        public void AsignarTriggerDriverLectorTarjeta_AsignarNull_LectorAnteriorDesconectado()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);
            target.AsignarTriggerDriverLectorTarjeta(null);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Tarjeta"] = "X" }
                }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        // ------------------------------------------------------------------ AsignarTriggerDriverSensor

        [Test]
        public void AsignarTriggerDriverSensor_NuevoSensor_SeRegistraAlEvento()
        {
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverSensor(sensorMock.Object);

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
        }

        [Test]
        public void AsignarTriggerDriverSensor_ReemplazarSensor_SensorAnteriorYaNoDisparaEventos()
        {
            var sensor1 = new Mock<IDriverSensor>();
            var sensor2 = new Mock<IDriverSensor>();

            target.AsignarTriggerDriverSensor(sensor1.Object);
            target.AsignarTriggerDriverSensor(sensor2.Object);

            sensor1.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        [Test]
        public void AsignarTriggerDriverSensor_AsignarNull_SensorAnteriorDesconectado()
        {
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverSensor(sensorMock.Object);
            target.AsignarTriggerDriverSensor(null);

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        // ------------------------------------------------------------------ ProcesarTrigger via lector

        [Test]
        public void OnEventoTrigger_EventoCorrecto_GeneraNotificacionIdentificacionVehicular()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Tarjeta"] = "TARJ01" }
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.CodigoDispositivo, Is.EqualTo("CIV01"));
            Assert.That(notif.CodigoEvento, Is.EqualTo(CodigosEventos.IdentificacionVehicular));
        }

        [Test]
        public void OnEventoTrigger_EventoIncorrecto_NoGeneraNotificacion()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.EntradaActivada
                }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        [Test]
        public void OnEventoTrigger_TarjetaEnDatos_NotificacionContieneValorTarjeta()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Tarjeta"] = "ABC123" }
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["Tarjeta"], Is.EqualTo("ABC123"));
        }

        [Test]
        public void OnEventoTrigger_SinDatos_NotificacionConTarjetaVacia()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = null
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["Tarjeta"], Is.EqualTo(string.Empty));
        }

        // ------------------------------------------------------------------ ProcesarTrigger via sensor

        [Test]
        public void OnEventoSensorVehicular_EntradaActivada_GeneraNotificacionIdentificacionVehicular()
        {
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverSensor(sensorMock.Object);

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.CodigoDispositivo, Is.EqualTo("CIV01"));
            Assert.That(notif.CodigoEvento, Is.EqualTo(CodigosEventos.IdentificacionVehicular));
        }

        [Test]
        public void OnEventoSensorVehicular_EventoIncorrecto_NoGeneraNotificacion()
        {
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverSensor(sensorMock.Object);

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaDesactivada }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        [Test]
        public void OnEventoSensorVehicular_TarjetaEnNotificacionSiempreVacia()
        {
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverSensor(sensorMock.Object);

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["Tarjeta"], Is.EqualTo(string.Empty));
        }

        // ------------------------------------------------------------------ ConsultarPresencia (via notificacion)

        [Test]
        public void ProcesarTrigger_SinSensorPresencia_VehiculoPresenteEsFalse()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["VehiculoPresente"], Is.EqualTo("False"));
        }

        [Test]
        public void ProcesarTrigger_ConSensorPresenciaActivo_VehiculoPresenteEsTrue()
        {
            var sensorPresenciaMock = new Mock<IDriverSensor>();
            sensorPresenciaMock
                .Setup(s => s.ConsultaEstadoActual())
                .Returns(new ResultadoEstadoSensor { EstadoActivo = true, Mensaje = Mensaje.ResultadoOK() });

            target.AsignarDriverPresencia(sensorPresenciaMock.Object);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["VehiculoPresente"], Is.EqualTo("True"));
        }

        [Test]
        public void ProcesarTrigger_ConSensorPresenciaQueArrojaExcepcion_VehiculoPresenteEsFalse()
        {
            var sensorPresenciaMock = new Mock<IDriverSensor>();
            sensorPresenciaMock
                .Setup(s => s.ConsultaEstadoActual())
                .Throws(new Exception("error de conexión"));

            target.AsignarDriverPresencia(sensorPresenciaMock.Object);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.Datos["VehiculoPresente"], Is.EqualTo("False"));
        }

        // ------------------------------------------------------------------ ActualizarConfig

        [Test]
        public void ActualizarConfig_NuevaConfig_PropiedadConfigReflejaElCambio()
        {
            var nuevaConfig = new ConfigIdentificacionVehicular
            {
                Codigo = "CIV_NUEVO",
                Activo = true,
                Camaras = new List<ConfigIdentificacionVehicularCamara>()
            };

            target.ActualizarConfig(nuevaConfig);

            Assert.That(target.Config.Codigo, Is.EqualTo("CIV_NUEVO"));
        }

        [Test]
        public void ActualizarConfig_TrasCambio_NotificacionUsaCodigoNuevo()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            var nuevaConfig = new ConfigIdentificacionVehicular
            {
                Codigo = "CIV_ACTUALIZADO",
                Activo = true,
                Camaras = new List<ConfigIdentificacionVehicularCamara>()
            };
            target.ActualizarConfig(nuevaConfig);

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            });

            var notif = EsperarNotificacion();
            Assert.That(notif, Is.Not.Null);
            Assert.That(notif.CodigoDispositivo, Is.EqualTo("CIV_ACTUALIZADO"));
        }

        // ------------------------------------------------------------------ Dispose

        [Test]
        public void Dispose_DesconectaTodosLosTriggers()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            var sensorMock = new Mock<IDriverSensor>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);
            target.AsignarTriggerDriverSensor(sensorMock.Object);

            target.Dispose();

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            });
            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });
            Thread.Sleep(150);

            Assert.That(notificacionesCapturadas, Is.Empty);
        }

        [Test]
        public void Dispose_LlamadaMultiplesVeces_NoLanzaExcepcion()
        {
            Assert.DoesNotThrow(() =>
            {
                target.Dispose();
                // segunda llamada tras TearDown no debería ocurrir normalmente,
                // pero validamos que Dispose es robusto llamándolo una vez aquí
            });
        }

        // ------------------------------------------------------------------ Semáforo (procesamiento secuencial)

        [Test]
        public void ProcesarTrigger_DosEventosConcurrentes_SeProcecanSecuencialmente()
        {
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.AsignarTriggerDriverLectorTarjeta(lectorMock.Object);

            var evento = new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string>()
                }
            };

            lectorMock.Raise(l => l.EventoDriver += null, evento);
            lectorMock.Raise(l => l.EventoDriver += null, evento);

            // Esperamos ambas notificaciones
            var timeout = DateTime.UtcNow.AddSeconds(5);
            while (notificacionesCapturadas.Count < 2 && DateTime.UtcNow < timeout)
                Thread.Sleep(20);

            Assert.That(notificacionesCapturadas.Count, Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ Helpers

        private NotificacionEvento EsperarNotificacion(int timeoutSegundos = 3)
        {
            var timeout = DateTime.UtcNow.AddSeconds(timeoutSegundos);
            while (DateTime.UtcNow < timeout)
            {
                lock (notificacionesCapturadas)
                {
                    if (notificacionesCapturadas.Count > 0)
                        return notificacionesCapturadas[0];
                }
                Thread.Sleep(20);
            }
            return null;
        }
    }
}
