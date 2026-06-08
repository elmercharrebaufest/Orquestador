using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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
    public class AdministradorIdentificacionVehicularTest
    {
        private AdministradorIdentificacionVehicular target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IServicioRemotoFactory> servicioRemotoFactoryMock;

        private const int IdOrquestador = 10;

        [SetUp]
        public void SetUp()
        {
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioMock = new Mock<IRepositorio>();
            servicioRemotoFactoryMock = new Mock<IServicioRemotoFactory>();

            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

            target = new AdministradorIdentificacionVehicular(
                repositorioFactoryMock.Object,
                servicioRemotoFactoryMock.Object,
                new NullLogger());
        }

        // ------------------------------------------------------------------ Iniciar

        [Test]
        public void Iniciar_SinCIVsDisponibles_LiberaYNoTomaninguno()
        {
            repositorioMock
                .Setup(r => r.Listar<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(new List<ConfigIdentificacionVehicular>());

            target.Iniciar(IdOrquestador);

            repositorioMock.Verify(r => r.LiberarCIVs(IdOrquestador), Times.Once());
            repositorioMock.Verify(r => r.TomarCIV(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void Iniciar_ConCIVDisponible_TomaCIV()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            SetupListarCIVs(civ);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, "CIV01")).Returns(true);

            target.Iniciar(IdOrquestador);

            repositorioMock.Verify(r => r.TomarCIV(IdOrquestador, "CIV01"), Times.Once());
        }

        [Test]
        public void Iniciar_CIVNoSePuedeTomar_NoGeneraDriver()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            SetupListarCIVs(civ);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, "CIV01")).Returns(false);

            target.Iniciar(IdOrquestador);

            // No debe quedar pendiente: conectar el lector no debe disparar notificaciones
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);
            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(100);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        // ------------------------------------------------------------------ Detener

        [Test]
        public void Detener_LiberaCIVsEnRepo()
        {
            SetupListarCIVs();
            target.Iniciar(IdOrquestador);

            target.Detener();

            // LiberarCIVs debe llamarse en Iniciar + en Detener
            repositorioMock.Verify(r => r.LiberarCIVs(IdOrquestador), Times.Exactly(2));
        }

        [Test]
        public void Detener_ConDriverActivo_NoGeneraExcepcion()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            Assert.DoesNotThrow(() => target.Detener());
        }

        // ------------------------------------------------------------------ ConectarDriver

        [Test]
        public void ConectarDriver_DispositivoNoEsCIV_NoGeneraNotificaciones()
        {
            SetupListarCIVs();
            target.Iniciar(IdOrquestador);

            var lectorDesconocido = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("DISP_DESCONOCIDO", lectorDesconocido.Object);

            lectorDesconocido.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(100);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ConectarDriver_TriggerDisponibleConCIVPendiente_CreaDriverYProcesaEventos()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            var notificacionCapturada = CapturarNotificacion();

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "TARJ01" }
                }
            });

            EsperarNotificacion(notificacionCapturada);

            Assert.That(notificacionCapturada.Capturada, Is.Not.Null);
            Assert.That(notificacionCapturada.Capturada.CodigoDispositivo, Is.EqualTo("CIV01"));
        }

        [Test]
        public void ConectarDriver_CIVConSensorVehicular_CreaDriverAlConectarSensor()
        {
            var civ = CivSoloSensor("CIV01", "SENS01");
            IniciarConCiv(civ);

            var sensorMock = new Mock<IDriverSensor>();
            target.ConectarDriver("SENS01", sensorMock.Object);

            var notificacionCapturada = CapturarNotificacion();

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });

            EsperarNotificacion(notificacionCapturada);

            Assert.That(notificacionCapturada.Capturada, Is.Not.Null);
            Assert.That(notificacionCapturada.Capturada.CodigoDispositivo, Is.EqualTo("CIV01"));
        }

        [Test]
        public void ConectarDriver_TriggerPrincipalNoDisponible_CIVSiguePendiente()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var otroLector = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("OTRO", otroLector.Object);

            otroLector.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(100);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void ConectarDriver_DriverYaActivo_ReconectaTriggerEnDriverExistente()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock1 = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock1.Object);

            var lectorMock2 = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock2.Object);

            // lector1 ya no debe producir notificaciones (desconectado)
            lectorMock1.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "OLD" }
                }
            });
            Thread.Sleep(100);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());

            // lector2 SI debe producir notificaciones
            var notificacionCapturada = CapturarNotificacion();
            lectorMock2.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "NEW" }
                }
            });
            EsperarNotificacion(notificacionCapturada);
            Assert.That(notificacionCapturada.Capturada, Is.Not.Null);
        }

        // ------------------------------------------------------------------ DesconectarDriver

        [Test]
        public void DesconectarDriver_DriverNoRegistrado_NoHaceNada()
        {
            SetupListarCIVs();
            target.Iniciar(IdOrquestador);

            Assert.DoesNotThrow(() => target.DesconectarDriver("DISP_DESCONOCIDO"));
        }

        [Test]
        public void DesconectarDriver_LectorActivo_DejaDeRecibirEventos()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);
            target.DesconectarDriver("LECT01");

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(150);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void DesconectarDriver_SensorActivo_DejaDeRecibirEventos()
        {
            var civ = CivSoloSensor("CIV01", "SENS01");
            IniciarConCiv(civ);

            var sensorMock = new Mock<IDriverSensor>();
            target.ConectarDriver("SENS01", sensorMock.Object);
            target.DesconectarDriver("SENS01");

            sensorMock.Raise(s => s.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento { CodigoEvento = CodigosEventos.EntradaActivada }
            });
            Thread.Sleep(150);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        // ------------------------------------------------------------------ RecargarConfig

        [Test]
        public void RecargarConfig_CIVNoExisteEnRepo_NoGeneraExcepcion()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns((ConfigIdentificacionVehicular)null);

            Assert.DoesNotThrow(() => target.RecargarConfig("CIV01"));
        }

        [Test]
        public void RecargarConfig_CIVInactivo_DejaDeRecibirEventos()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            var civInactivo = CivSoloLector("CIV01", "LECT01");
            civInactivo.Activo = false;
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(civInactivo);

            target.RecargarConfig("CIV01");

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(150);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RecargarConfig_CIVActivoTomadoPorOtro_DejaDeRecibirEventos()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            var civActivo = CivSoloLector("CIV01", "LECT01");
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(civActivo);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, "CIV01")).Returns(false);

            target.RecargarConfig("CIV01");

            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "X" }
                }
            });
            Thread.Sleep(150);
            servicioRemotoFactoryMock.Verify(f => f.CrearServicioSuscriptor(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void RecargarConfig_CIVActivo_ActualizaDriverExistente()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            var civActualizado = CivSoloLector("CIV01", "LECT01");
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(civActualizado);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, "CIV01")).Returns(true);

            // Debe actualizar el driver sin destruirlo
            Assert.DoesNotThrow(() => target.RecargarConfig("CIV01"));

            // El driver actualizado debe seguir procesando eventos
            var notificacionCapturada = CapturarNotificacion();
            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "POST_UPDATE" }
                }
            });
            EsperarNotificacion(notificacionCapturada);
            Assert.That(notificacionCapturada.Capturada, Is.Not.Null);
        }

        [Test]
        public void RecargarConfig_CIVNuevoActivo_CreaDriverCuandoTriggerDisponible()
        {
            SetupListarCIVs();
            target.Iniciar(IdOrquestador);

            // CIV llega via RecargarConfig
            var civNuevo = CivSoloLector("CIV01", "LECT01");
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(civNuevo);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, "CIV01")).Returns(true);

            target.RecargarConfig("CIV01");

            // Conectar el lector que actua como trigger
            var lectorMock = new Mock<IDriverLectorTarjetas>();
            target.ConectarDriver("LECT01", lectorMock.Object);

            var notificacionCapturada = CapturarNotificacion();
            lectorMock.Raise(l => l.EventoDriver += null, new EventoDriverEventArgs
            {
                Notificacion = new NotificacionEvento
                {
                    CodigoEvento = CodigosEventos.LecturaTarjetaRecibida,
                    Datos = new Dictionary<string, string> { ["Valor"] = "NUEVO" }
                }
            });
            EsperarNotificacion(notificacionCapturada);
            Assert.That(notificacionCapturada.Capturada, Is.Not.Null);
            Assert.That(notificacionCapturada.Capturada.CodigoDispositivo, Is.EqualTo("CIV01"));
        }

        // ------------------------------------------------------------------ Suscribir

        [Test]
        public void Suscribir_CIVInexistente_RetornaDispositivoInexistente()
        {
            SetupListarCIVs();
            target.Iniciar(IdOrquestador);

            var resultado = target.Suscribir("CIV_INEXISTENTE", CodigosEventos.IdentificacionVehicular, "http://test/suscriptor");

            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.DispositivoInexistente));
        }

        [Test]
        public void Suscribir_CIVExistente_CreaYRetornaIdSuscripcion()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            repositorioMock
                .Setup(r => r.Obtener<SuscripcionIdentificacionVehicular>(It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>()))
                .Returns((SuscripcionIdentificacionVehicular)null);
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(new ConfigIdentificacionVehicular { Codigo = "CIV01" });
            repositorioMock
                .Setup(r => r.Agregar(It.IsAny<SuscripcionIdentificacionVehicular>()))
                .Callback<SuscripcionIdentificacionVehicular>(s => s.Id = 99)
                .Returns<SuscripcionIdentificacionVehicular>(s => s);

            var resultado = target.Suscribir("CIV01", CodigosEventos.IdentificacionVehicular, "http://test/suscriptor");

            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(99));
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void Suscribir_SuscripcionYaExistente_RetornaIdExistenteSinCrear()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var suscripcionExistente = new SuscripcionIdentificacionVehicular
            {
                Id = 42,
                CodigoEvento = CodigosEventos.IdentificacionVehicular,
                RutaAccesoSuscriptor = "http://test/suscriptor"
            };
            repositorioMock
                .Setup(r => r.Obtener<SuscripcionIdentificacionVehicular>(It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>()))
                .Returns(suscripcionExistente);

            var resultado = target.Suscribir("CIV01", CodigosEventos.IdentificacionVehicular, "http://test/suscriptor");

            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.IdSuscripcion, Is.EqualTo(42));
            repositorioMock.Verify(r => r.Agregar(It.IsAny<SuscripcionIdentificacionVehicular>()), Times.Never());
        }

        // ------------------------------------------------------------------ CancelarSuscripcion

        [Test]
        public void CancelarSuscripcion_SuscripcionExistente_EliminaYRetornaOK()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            var suscripcion = new SuscripcionIdentificacionVehicular { Id = 7 };
            repositorioMock
                .Setup(r => r.Obtener<SuscripcionIdentificacionVehicular>(It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>()))
                .Returns(suscripcion);

            var resultado = target.CancelarSuscripcion("CIV01", "IdentificacionVehicular", "http://test/suscriptor");

            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            repositorioMock.Verify(r => r.Remover(suscripcion), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
        }

        [Test]
        public void CancelarSuscripcion_SuscripcionInexistente_RetornaError()
        {
            var civ = CivSoloLector("CIV01", "LECT01");
            IniciarConCiv(civ);

            repositorioMock
                .Setup(r => r.Obtener<SuscripcionIdentificacionVehicular>(It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>()))
                .Returns((SuscripcionIdentificacionVehicular)null);

            var resultado = target.CancelarSuscripcion("CIV01", "IdentificacionVehicular", "http://test/suscriptor");

            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.Error));
            repositorioMock.Verify(r => r.Remover(It.IsAny<SuscripcionIdentificacionVehicular>()), Times.Never());
        }

        // ------------------------------------------------------------------ Helpers

        private void IniciarConCiv(ConfigIdentificacionVehicular civ)
        {
            SetupListarCIVs(civ);
            repositorioMock.Setup(r => r.TomarCIV(IdOrquestador, civ.Codigo)).Returns(true);
            target.Iniciar(IdOrquestador);
        }

        private void SetupListarCIVs(params ConfigIdentificacionVehicular[] civs)
        {
            repositorioMock
                .Setup(r => r.Listar<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(new List<ConfigIdentificacionVehicular>(civs));
        }

        private NotificacionHolder CapturarNotificacion(string codigoCIV = "CIV01")
        {
            var holder = new NotificacionHolder();

            // Setup BD para Suscribir
            repositorioMock
                .Setup(r => r.Obtener<SuscripcionIdentificacionVehicular>(It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>()))
                .Returns((SuscripcionIdentificacionVehicular)null);
            repositorioMock
                .Setup(r => r.Obtener<ConfigIdentificacionVehicular>(It.IsAny<Expression<Func<ConfigIdentificacionVehicular, bool>>>()))
                .Returns(new ConfigIdentificacionVehicular { Codigo = codigoCIV });
            repositorioMock
                .Setup(r => r.Agregar(It.IsAny<SuscripcionIdentificacionVehicular>()))
                .Returns<SuscripcionIdentificacionVehicular>(s => s);

            // Setup BD para NotificarCIV
            repositorioMock
                .Setup(r => r.Listar<SuscripcionIdentificacionVehicular, string>(
                    It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, bool>>>(),
                    It.IsAny<Expression<Func<SuscripcionIdentificacionVehicular, string>>>()))
                .Returns(new List<string> { "http://test/suscriptor" });

            // Setup suscriptor remoto
            var suscriptorMock = new Mock<IServicioSuscriptor>();
            suscriptorMock
                .Setup(s => s.Recibir(It.IsAny<NotificacionEvento>()))
                .Callback<NotificacionEvento>(n => holder.Capturada = n);
            var clienteMock = new ClienteServicio<IServicioSuscriptor>(suscriptorMock.Object);
            servicioRemotoFactoryMock
                .Setup(f => f.CrearServicioSuscriptor(It.IsAny<string>()))
                .Returns(clienteMock);

            target.Suscribir(codigoCIV, CodigosEventos.IdentificacionVehicular, "http://test/suscriptor");
            return holder;
        }

        private static void EsperarNotificacion(NotificacionHolder holder, int timeoutSegundos = 3)
        {
            var timeout = DateTime.UtcNow.AddSeconds(timeoutSegundos);
            while (holder.Capturada == null && DateTime.UtcNow < timeout)
                Thread.Sleep(20);
        }

        private static ConfigIdentificacionVehicular CivSoloLector(string codigoCIV, string codigoLector)
        {
            var dispositivo = new Dispositivo { Codigo = codigoLector };
            var configLector = new ConfigLectorTarjetas { Dispositivo = dispositivo };
            return new ConfigIdentificacionVehicular
            {
                Codigo = codigoCIV,
                Activo = true,
                ConfigLectorTarjetas = configLector,
                Camaras = new List<ConfigIdentificacionVehicularCamara>()
            };
        }

        private static ConfigIdentificacionVehicular CivSoloSensor(string codigoCIV, string codigoSensor)
        {
            var dispositivo = new Dispositivo { Codigo = codigoSensor };
            var configSensor = new ConfigSensor { Dispositivo = dispositivo };
            return new ConfigIdentificacionVehicular
            {
                Codigo = codigoCIV,
                Activo = true,
                ConfigSensorVehicular = configSensor,
                Camaras = new List<ConfigIdentificacionVehicularCamara>()
            };
        }

        private class NotificacionHolder
        {
            public NotificacionEvento Capturada { get; set; }
        }
    }
}
