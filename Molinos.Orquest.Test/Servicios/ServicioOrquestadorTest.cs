using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Repositorio;
using Molinos.Orquest.Servicios;
using Molinos.Orquest.Servicios.Impl;
using Molinos.Orquest.Servicios.Procesamiento;
using Molinos.Orquest.Test.Mocks;
using Moq;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class ServicioOrquestadorTest
    {
        private ServicioOrquestador target;
        private Mock<IProcesadorFactory> procesadorFactoryMock;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IServicioRemotoFactory> orquestadorFactoryMock;
        private Mock<IServicioOrquestador> orquestadorRemotoMock;
        private INamedLocker namedLocker;
        private Mock<IProgramadorTareas> programadorMock;

        private Mock<IRepositorio> repositorioMock;
        private Mock<IProcesadorDispositivo> procesadorDispMock;

        private Action<IProcesadorDispositivo> callbackFinProcesamiento;

        [SetUp]
        public void SetUp()
        {
            procesadorFactoryMock = new Mock<IProcesadorFactory>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            orquestadorFactoryMock = new Mock<IServicioRemotoFactory>();
            namedLocker = new NamedLocker();
            programadorMock = new Mock<IProgramadorTareas>();

            target = new ServicioOrquestador(
                repositorioFactoryMock.Object,
                orquestadorFactoryMock.Object,
                procesadorFactoryMock.Object,
                namedLocker,
                programadorMock.Object,
                new NullLogger());

            procesadorDispMock = new Mock<IProcesadorDispositivo>();
            orquestadorRemotoMock = new Mock<IServicioOrquestador>();
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock.Setup(f => f.Repositorio()).Returns(repositorioMock.Object);

            procesadorFactoryMock
                .Setup(p => p.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()))
                .Returns<Dispositivo, Action<IProcesadorDispositivo>>((disp, callback) =>
                    {
                        callbackFinProcesamiento = callback;
                        return procesadorDispMock.Object;
                    });

            repositorioMock.Setup(r => r.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns(true);
        }

        [Test]
        public void TestInicializarOrquestadorNoRegistrado()
        {
            Orquestador orquestador = null;
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                orquestador = orq;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Orquestador>()), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());

            Assert.That(target.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(target.IdOrquestador, Is.EqualTo(5));
            Assert.That(target.UrlServicio, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));

            Assert.That(orquestador, Is.Not.Null);
            Assert.That(orquestador.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(orquestador.RutaAcceso, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));
        }

        [Test]
        public void TestInicializarOrquestadorRegistradoSinDispositivos()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://otroorquestador.com:8080/ServicioOrquestador"
            };

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Orquestador, bool>>>()))
                           .Returns<Expression<Func<Orquestador, bool>>>(
                               condicion => condicion.Compile().Invoke(orquestador) ? orquestador : null);
            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Orquestador>()), Times.Never());
            repositorioMock.Verify(r => r.LiberarDispositivos(5), Times.Once());

            Assert.That(target.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(target.IdOrquestador, Is.EqualTo(5));
            Assert.That(target.UrlServicio, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));

            Assert.That(orquestador, Is.Not.Null);
            Assert.That(orquestador.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(orquestador.RutaAcceso, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));
        }

        [Test]
        public void TestInicializarOrquestadorRegistradoConDispositivos()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://otroorquestador.com:8080/ServicioOrquestador"
            };

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Orquestador, bool>>>()))
                           .Returns<Expression<Func<Orquestador, bool>>>(
                               condicion => condicion.Compile().Invoke(orquestador) ? orquestador : null);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            repositorioMock.Verify(r => r.Agregar(It.IsAny<Orquestador>()), Times.Never());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivos(5), Times.Once());

            Assert.That(target.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(target.IdOrquestador, Is.EqualTo(5));
            Assert.That(target.UrlServicio, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));

            Assert.That(orquestador, Is.Not.Null);
            Assert.That(orquestador.NombreMaquina, Is.EqualTo("MIMAQUINA"));
            Assert.That(orquestador.RutaAcceso, Is.EqualTo("http://orquestadpor.com:8080/ServicioOrquestador"));
        }

        [Test]
        public void TestIniciarOrquestadorVerificarDispositivos()
        {
            Orquestador orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestadpor.com:8080/ServicioOrquestador"
            };
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(
                orq =>
                    {
                        orq.Id = 5;
                        return orq;
                    });

            var cabezal = new Dispositivo { Id = 1, Codigo = "BAL01", Activo = true };
            var humedimetro = new Dispositivo { Id = 2, Codigo = "HUM01", Activo = true };
            var otroCabezal = new Dispositivo { Id = 3, Codigo = "BAL02", Activo = false };

            var dispositivos = new List<Dispositivo> { cabezal, humedimetro, otroCabezal };

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>, Expression<Func<Dispositivo, string>>>(
                               (filtro, seleccion) => dispositivos.Where(filtro.Compile()).Select(seleccion.Compile()).ToList());

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                                condicion => dispositivos.SingleOrDefault(condicion.Compile()));

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Listar<Estado>(null)).Returns(new List<Estado>());

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() });

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");
            target.VerificarDispositivos();
            procesadorDispMock.Verify(p => p.Procesar(It.Is<Comando>(cmd => cmd.GetType() == typeof(EjecutarVerificacionDispositivo))), Times.Exactly(2));
        }

        [Test]
        public void TestIniciarOrquestadorVerificarDispositivosUnoFalla()
        {
            Orquestador orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestadpor.com:8080/ServicioOrquestador"
            };
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(
                orq =>
                {
                    orq.Id = 5;
                    return orq;
                });

            var cabezal = new Dispositivo { Id = 1, Codigo = "BAL01", Activo = true };
            var humedimetro = new Dispositivo { Id = 2, Codigo = "HUM01", Activo = true };
            var otroCabezal = new Dispositivo { Id = 3, Codigo = "BAL02", Activo = false };

            var dispositivos = new List<Dispositivo> { cabezal, humedimetro, otroCabezal };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);
            repositorioMock.Setup(r => r.Listar<Estado>(null)).Returns(new List<Estado>());

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>, Expression<Func<Dispositivo, string>>>(
                               (filtro, seleccion) => dispositivos.Where(filtro.Compile()).Select(seleccion.Compile()).ToList());

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                                condicion => dispositivos.SingleOrDefault(condicion.Compile()));

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);


            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                  .Returns<Comando>((cmd) => cmd.CodigoDispositivo == "BAL01"
                                                                  ? new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }
                                                                  : new ResultadoEjecutar { Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "Error") });

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");
            target.VerificarDispositivos();
            procesadorDispMock.Verify(p => p.Procesar(It.Is<Comando>(cmd => cmd.GetType() == typeof(EjecutarVerificacionDispositivo))), Times.Exactly(2));
        }

        [Test]
        public void TestDetenerOrquestador()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            target.Detener();

            repositorioMock.Verify(r => r.Remover<Orquestador>(5), Times.Once());
            repositorioMock.Verify(r => r.GuardarCambios(), Times.AtLeastOnce());
            repositorioMock.Verify(r => r.LiberarDispositivos(5), Times.Once());

            Assert.That(target.NombreMaquina, Is.Null);
            Assert.That(target.IdOrquestador, Is.EqualTo(0));
            Assert.That(target.UrlServicio, Is.Null);
        }

        [Test]
        public void TestEjecutarEnDispositivoLibre()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                              .Returns<Comando>((cmd) =>
                                  {
                                      repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
                                      return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje",
                                                                                                             1500);
                                  });
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);


            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            Thread.Sleep(20);
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());
        }

        [Test]
        public void TestEjecutarEnDispositivoLibreCodigoDispCaseInsensitive()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                              .Returns<Comando>((cmd) =>
                              {
                                  repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
                                  return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje",
                                                                                                         1500);
                              });
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);


            var comando = new EjecutarPesaje { CodigoDispositivo = "baldm01" };
            var resultado = target.Ejecutar(comando);

            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            Thread.Sleep(20);
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());
        }

        [Test]
        public void TestEjecutarEnDispositivoLibreQueSeMantieneTomado()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                              .Returns<Comando>((cmd) =>
                              {
                                  repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
                                  return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje",
                                                                                                         1500);
                              });

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");


            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            Thread.Sleep(20);
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Never());
        }

        [Test]
        public void TestEjecutarEnDispositivoTomadoPorMismoOrquestador()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var comando2 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.Setup(proc => proc.Procesar(comando2))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //Segundo comando con el dispositivo ya tomado.
            var resultado = target.Ejecutar(comando2);

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Once());

            //Ejecutamos el fin de procesamiento de la cola
            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));
        }


        [Test]
        public void TestEjecutarEnDispositivoTomadoPorOtroOrquestadorQueResponde()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };


            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado("BALDM01")).Returns(otroOrquestador);

            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = otroOrquestador,
                Activo = true
            };

            repositorioMock.Setup(s => s.ObtenerCodigoDispositivoConcentrador(It.IsAny<string>())).Returns("BALDM01");

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(false);
            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };


            orquestadorRemotoMock.Setup(servicio => servicio.Ejecutar(comando))
                                 .Returns<Comando>(cmd => new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            repositorioMock.Verify(r => r.LiberarDispositivos(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void TestEjecutarEnDispositivoTomadoPorOtroOrquestadorQueNoResponde()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado("BALDM01")).Returns(otroOrquestador);

            var cabezal = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = otroOrquestador,
                Activo = true
            };

            var cabezal2 = new Dispositivo
            {
                Codigo = "BALDM02",
                Descripcion = "OTra Cabezal Dummy",
                TomadoPor = otroOrquestador,
                Activo = true
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(cabezal) ? cabezal : null);

            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };


            orquestadorRemotoMock.Setup(servicio => servicio.Ejecutar(comando))
            .Returns<ComandoEjecutar>(cmd =>
                {
                    //simulamos que el orquestador remoto no responde...
                    throw new WebException("Simulación de error al conectar con orquestador remoto!!");
                });

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");

            var libre = false;

            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns<int, string>((id, cod) => libre);
            repositorioMock.Setup(r => r.LiberarDispositivos(10)).Callback<int>(id =>
                {
                    cabezal.TomadoPor = null;
                    cabezal2.TomadoPor = null;
                    libre = true;
                });

            repositorioMock.Setup(s => s.ObtenerCodigoDispositivoConcentrador(It.IsAny<string>())).Returns("BALDM01");
            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            repositorioMock.Verify(r => r.LiberarDispositivos(10), Times.Once());

        }

        [Test]
        public void TestEjecutarEnDispositivoInexistente()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true
            };
            repositorioMock.Setup(r => r.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                .Returns<Expression<Func<Dispositivo, bool>>>(condicion => condicion.Compile().Invoke(dispositivo));

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDMXX" };
            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.DispositivoInexistente));
            Assert.That(resultado.Valores, Is.Not.Null);
        }

        [Test]
        public void TestEjecutarEnDispositivoInactivo()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = false
            };
            repositorioMock.Setup(r => r.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                .Returns<Expression<Func<Dispositivo, bool>>>(condicion => condicion.Compile().Invoke(dispositivo));
            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.DispositivoInexistente));
            Assert.That(resultado.Valores, Is.Not.Null);
        }

        [Test]
        public void TestEjecutarConErrorDesconocido()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(true);

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");
            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Throws<Exception>();

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.Error));
            Assert.That(resultado.Valores, Is.Empty);
        }

        [Test]
        public void TestEjecutarDriverNoEncontrado()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Configuracion = new ConfigCabezal
                {
                    ClaseDriver = "MiClase",
                },
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorFactoryMock.Setup(
                f => f.ProcesadorParaDispositivo(dispositivo, It.IsAny<Action<IProcesadorDispositivo>>()))
                                 .Throws(new DriverNoEncontradoException("No encontrado", dispositivo.Codigo, dispositivo.Configuracion.ClaseDriver));


            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.DriverNoEncontrado));
            Assert.That(resultado.Valores, Is.Empty);

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());
        }

        [Test]
        public void TestEjecutarTipoDispositivoIncorrecto()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Configuracion = new ConfigCabezal
                {
                    ClaseDriver = "MiClase",
                },
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorFactoryMock.Setup(
                f => f.ProcesadorParaDispositivo(dispositivo, It.IsAny<Action<IProcesadorDispositivo>>()))
                                 .Throws(new TipoDispositivoIncorrectoException("incorrecto", dispositivo.Codigo, dispositivo.Configuracion.GetType().Name));


            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.TipoDispositivoIncorrecto));
            Assert.That(resultado.Valores, Is.Empty);

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());
        }

        [Test]
        public void TestEjecutarTipoDriverIncorrecto()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Configuracion = new ConfigCabezal
                {
                    ClaseDriver = "MiClase",
                },
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            procesadorFactoryMock.Setup(
                f => f.ProcesadorParaDispositivo(dispositivo, It.IsAny<Action<IProcesadorDispositivo>>()))
                                 .Throws(new TipoDriverIncorrectoException("incorrecto", dispositivo.Codigo, dispositivo.Configuracion.ClaseDriver));


            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.TipoDriverIncorrecto));
            Assert.That(resultado.Valores, Is.Empty);

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());
        }

        [Test]
        public void TestDepurarSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var fecha = DateTime.Now;
            var dispositivos = new List<Dispositivo>
                {
                    new Dispositivo {Id = 1, Codigo = "BAL01", Activo = true},
                    new Dispositivo {Id = 2, Codigo = "HUM02", Activo = true}
                };

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                .Returns<Expression<Func<Dispositivo, bool>>>(cond => dispositivos.SingleOrDefault(cond.Compile()));

            repositorioMock.Setup(r => r.DispositivosConSuscripcionesVencidas(fecha))
                           .Returns(new List<string> { "BAL01", "HUM02" });

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<ComandoDepurarSuscripciones>()))
                              .Returns<Comando>(cmd => new ResultadoComando { Mensaje = Mensaje.ResultadoOK() });

            programadorMock.Raise(p => p.DepurarSuscripciones += null, new DepurarSuscripcionesEventArgs { Vencimiento = fecha });

            procesadorDispMock.Verify(proc => proc.Procesar(It.Is<ComandoDepurarSuscripciones>(cmd => cmd.CodigoDispositivo == "BAL01" && cmd.Vencimiento == fecha)), Times.Once());
            procesadorDispMock.Verify(proc => proc.Procesar(It.Is<ComandoDepurarSuscripciones>(cmd => cmd.CodigoDispositivo == "HUM02" && cmd.Vencimiento == fecha)), Times.Once());
            procesadorDispMock.Verify(proc => proc.Procesar(It.IsAny<ComandoDepurarSuscripciones>()), Times.Exactly(2));
        }

        [Test]
        public void TestEjecutarEnDispositivoLogicoConConcentradorLibre()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var concentrador = new Dispositivo
            {
                Id = 10,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true
            };


            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = concentrador
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null).Callback(() =>
                               {
                                   // assign new value for second call
                                   dispositivo = concentrador;
                               });

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                              .Returns<Comando>((cmd) =>
                              {
                                  repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Once());
                                  return new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500);
                              });
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            // Verificamos para cuando se termino la ejecución el dispositivo se liberó
            Thread.Sleep(20);
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());
        }

        [Test]
        public void TestEjecutarEnDispositivoLogicoConConcentradorTomadoPorMismoOrquestador()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var concentrador = new Dispositivo
            {
                Id = 10,
                Codigo = "ITCDM01",
                TomadoPor = orquestador,
                EsConcentrador = true,
                Activo = true
            };

            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = concentrador
            };

            var dispositivo2 = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM02",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = concentrador
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => new[] { dispositivo, dispositivo2 }.SingleOrDefault(condicion.Compile())).Callback(() =>
                               {
                                   // assign new value for second call
                                   dispositivo = concentrador;
                               }); ;

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var comando2 = new EjecutarPesaje { CodigoDispositivo = "BALDM02" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.Setup(proc => proc.Procesar(comando2))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //Segundo comando con el dispositivo ya tomado.
            var resultado = target.Ejecutar(comando2);

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Once());

            //Ejecutamos el fin de procesamiento de la cola
            callbackFinProcesamiento(procesadorDispMock.Object);

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));
        }

        [Test]
        public void TestEjecutarEnDispositivoLogicoConconcentradorTomadoPorOtroOrquestadorQueResponde()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };


            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            var concentrador = new Dispositivo
            {
                Id = 10,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true
            };

            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                TomadoPor = otroOrquestador,
                Activo = true,
                Concentrador = concentrador
            };
            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado(concentrador.Codigo)).Returns(otroOrquestador);
            repositorioMock.Setup(s => s.ObtenerCodigoDispositivoConcentrador(It.IsAny<string>())).Returns(concentrador.Codigo);
            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null).Callback(() =>
                               {
                                   // assign new value for second call
                                   dispositivo = concentrador;
                               });

            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(false);
            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            orquestadorRemotoMock.Setup(servicio => servicio.Ejecutar(comando))
                                 .Returns<Comando>(cmd => new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            repositorioMock.Verify(r => r.LiberarDispositivos(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void TestEjecutarEnDispositivoLogicoConConcentradorTomadoPorOtroOrquestadorQueNoResponde()
        {
            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            var concentrador = new Dispositivo
            {
                Id = 10,
                Codigo = "ITCDM01",
                TomadoPor = orquestador,
                EsConcentrador = true,
                Activo = true
            };

            var cabezal = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                TomadoPor = otroOrquestador,
                Activo = true,
                Concentrador = concentrador
            };

            var cabezal2 = new Dispositivo
            {
                Codigo = "BALDM02",
                Descripcion = "OTra Cabezal Dummy",
                TomadoPor = otroOrquestador,
                Activo = true
            };



            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado(concentrador.Codigo)).Returns(otroOrquestador);
            repositorioMock.Setup(s => s.ObtenerCodigoDispositivoConcentrador(It.IsAny<string>())).Returns(concentrador.Codigo);
            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.SetupSequence(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns(cabezal).Returns(cabezal);

            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };


            orquestadorRemotoMock.Setup(servicio => servicio.Ejecutar(comando))
            .Returns<ComandoEjecutar>(cmd =>
            {
                //simulamos que el orquestador remoto no responde...
                throw new WebException("Simulación de error al conectar con orquestador remoto!!");
            });

            procesadorDispMock.Setup(proc => proc.Procesar(It.IsAny<Comando>()))
                .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 1500));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");

            var libre = false;
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns<int, string>((id, cod) => libre);
            repositorioMock.Setup(r => r.LiberarDispositivos(10)).Callback<int>(id =>
            {
                cabezal.TomadoPor = null;
                cabezal2.TomadoPor = null;
                libre = true;
            });

            var resultado = target.Ejecutar(comando);
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
            Assert.That(resultado.Valores, Is.Not.Null);
            Assert.That(resultado.Valores["Pesaje"], Is.EqualTo(1500M));

            repositorioMock.Verify(r => r.LiberarDispositivos(10), Times.Once());

        }

        [Test]
        public void TestEjecutarEnDispositivoLogicoConConcentradorInactivo()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var concentrador = new Dispositivo
            {
                Id = 10,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = false
            };

            var dispositivo = new Dispositivo
            {
                Id = 1,
                Codigo = "BALDM01",
                Descripcion = "Balanza Dummy",
                Activo = true,
                Concentrador = concentrador
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(false);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            var comando = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };
            var resultado = target.Ejecutar(comando);

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.DispositivoInexistente));
            Assert.That(resultado.Valores, Is.Empty);

            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Never());
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoLibre()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true
            };

            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
                           .Returns<int>(
                               id => dispositivo.Id == id ? dispositivo : null);

            var resultado = target.RecargarConfiguracion("BALDM01");
            procesadorDispMock.Verify(p => p.Dispose(), Times.Never());
            procesadorFactoryMock.Verify(p => p.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Never());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            orquestadorRemotoMock.Verify(o => o.RecargarConfiguracion(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOrquestador()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(
                   id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Exactly(2));
            procesadorDispMock.Verify(p => p.Dispose(), Times.Exactly(2));

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Exactly(2));

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOrquestadorConSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(
                   id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Exactly(2));
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOrquestadorConSuscripcionesDesactivado()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(
                   id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);

            dispositivo.Activo = false;
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Once());

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "BALDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "BALDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoLogicoTomadoPorOrquestadorConSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var itc = new Dispositivo
            {
                Id = 20,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true,
                TomadoPor = orquestador
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true,
                Concentrador = itc
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(
                   id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Exactly(2));
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoLogicoLibreConConcentradorTomadoPorOrquestadorConSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var itc = new Dispositivo
            {
                Id = 20,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true,
                TomadoPor = orquestador
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = null,
                Activo = true,
                Concentrador = itc
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(
                   id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Exactly(2));
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoLogicoTomadoPorOrquestadorConSuscripcionesDesactivado()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var itc = new Dispositivo
            {
                Id = 20,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true,
                TomadoPor = orquestador
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true,
                Concentrador = itc
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            itc.Activo = false;
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Once());

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Once());
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoLogicoDesactivadoTomadoPorOrquestadorConSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");


            var orquestador = new Orquestador
            {
                Id = 5,
                NombreMaquina = "MIMAQUINA",
                RutaAcceso = "http://orquestador.com:8080/ServicioOrquestador",
            };

            var itc = new Dispositivo
            {
                Id = 20,
                Codigo = "ITCDM01",
                EsConcentrador = true,
                Activo = true,
                TomadoPor = orquestador
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                TomadoPor = orquestador,
                Activo = true,
                Concentrador = itc
            };

            repositorioMock.Setup(r => r.Obtener<Orquestador>(5)).Returns(orquestador);
            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);
            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
               .Returns<int>(id => dispositivo.Id == id ? dispositivo : null);

            var comando1 = new EjecutarPesaje { CodigoDispositivo = "BALDM01" };

            procesadorDispMock.Setup(proc => proc.Procesar(comando1))
              .Returns(new ResultadoEjecutar { Mensaje = Mensaje.ResultadoOK() }.Agregar("Pesaje", 200));

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("ITCDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);
            repositorioMock.Setup(r => r.TomarDispositivo(5, "ITCDM01")).Returns(true);

            //Primer comando para que tome el dispositivo y se encole.
            target.Ejecutar(comando1);
            dispositivo.Activo = false;
            //recarga de configuracion con el dispositivo ya tomado
            var resultado = target.RecargarConfiguracion("BALDM01");

            // Se instancio una sola vez el procesador
            procesadorFactoryMock.Verify(f => f.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));

            procesadorDispMock.Verify(p => p.ProcesarRemanentes(), Times.Exactly(2));
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());

            // Verificamos que el dispositivo se tomo y liberó una sola vez
            repositorioMock.Verify(r => r.TomarDispositivo(5, "ITCDM01"), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivo(5, "ITCDM01"), Times.Once());

            //Verificamos que el resultado es correcto
            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOtroOrquestadorQueResponde()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true,
                TomadoPor = otroOrquestador
            };
            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado(dispositivo.Codigo)).Returns(otroOrquestador);
            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(false);
            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
                           .Returns<int>(
                               id => dispositivo.Id == id ? dispositivo : null);

            orquestadorRemotoMock.Setup(servicio => servicio.RecargarConfiguracion("BALDM01"))
                                 .Returns<string>(codigo => new ResultadoComando { Mensaje = Mensaje.ResultadoOK() });

            var resultado = target.RecargarConfiguracion("BALDM01");
            procesadorDispMock.Verify(p => p.Dispose(), Times.Never());
            procesadorFactoryMock.Verify(p => p.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Never());
            orquestadorRemotoMock.Verify(o => o.RecargarConfiguracion("BALDM01"), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
            orquestadorRemotoMock.Verify(o => o.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOtroOrquestadorQueNoRespondeSinSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true,
                TomadoPor = otroOrquestador
            };
            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado(dispositivo.Codigo)).Returns<string>(c => otroOrquestador);
            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);
            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
                           .Returns<int>(
                               id => dispositivo.Id == id ? dispositivo : null);

            orquestadorRemotoMock.Setup(servicio => servicio.RecargarConfiguracion("BALDM01"))
                                 .Returns<string>(codigo =>
                                 {
                                     //simulamos que el orquestador remoto no responde...
                                     throw new WebException("Simulación de error al conectar con orquestador remoto!!");
                                 });

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(true);

            var libre = false;
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns<int, string>((id, cod) => libre);
            repositorioMock.Setup(r => r.LiberarDispositivos(10)).Callback<int>(id =>
            {
                dispositivo.TomadoPor = null;
                libre = true;
                otroOrquestador = null;
            });

            var resultado = target.RecargarConfiguracion("BALDM01");
            procesadorDispMock.Verify(p => p.Dispose(), Times.Exactly(2));
            procesadorFactoryMock.Verify(p => p.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));
            orquestadorRemotoMock.Verify(o => o.RecargarConfiguracion("BALDM01"), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(2));
            repositorioMock.Verify(r => r.LiberarDispositivos(10), Times.Once());
        }

        [Test]
        public void TestRecargarConfiguracionEnDispositivoTomadoPorOtroOrquestadorQueNoRespondeConSuscripciones()
        {
            repositorioMock.Setup(r => r.Agregar(It.IsAny<Orquestador>())).Returns<Orquestador>(orq =>
            {
                orq.Id = 5;
                return orq;
            });

            repositorioMock.Setup(r => r.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>(), It.IsAny<Expression<Func<Dispositivo, string>>>()))
                           .Returns(new List<string>());

            target.Iniciar("MIMAQUINA", "http://orquestadpor.com:8080/ServicioOrquestador");

            var otroOrquestador = new Orquestador
            {
                Id = 10,
                NombreMaquina = "OTRAMAQUINA",
                RutaAcceso = "http://otro.com:8080/ServicioOrquestador",
            };

            var dispositivo = new Dispositivo
            {
                Id = 2,
                Codigo = "BALDM01",
                Descripcion = "Cabezal Dummy",
                Activo = true,
                TomadoPor = otroOrquestador
            };
            repositorioMock.Setup(r => r.OrquestadorQueTieneTomado(dispositivo.Codigo)).Returns<string>(c => otroOrquestador);
            repositorioMock.Setup(r => r.TomarDispositivo(5, It.IsAny<string>())).Returns(true);
            orquestadorFactoryMock.Setup(factory => factory.CrearServicioOrquestador(otroOrquestador.RutaAcceso))
                                  .Returns(new ClienteServicio<IServicioOrquestador>(orquestadorRemotoMock.Object));

            repositorioMock.Setup(r => r.Obtener(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                           .Returns<Expression<Func<Dispositivo, bool>>>(
                               condicion => condicion.Compile().Invoke(dispositivo) ? dispositivo : null);

            repositorioMock.Setup(r => r.Obtener<Dispositivo>(It.IsAny<int>()))
                           .Returns<int>(
                               id => dispositivo.Id == id ? dispositivo : null);

            orquestadorRemotoMock.Setup(servicio => servicio.RecargarConfiguracion("BALDM01"))
                                 .Returns<string>(codigo =>
                                 {
                                     //simulamos que el orquestador remoto no responde...
                                     throw new WebException("Simulación de error al conectar con orquestador remoto!!");
                                 });

            procesadorDispMock.SetupGet(p => p.CodigoDispositivo).Returns("BALDM01");
            procesadorDispMock.Setup(p => p.ProcesarRemanentes()).Returns(false);

            var libre = false;
            repositorioMock.Setup(r => r.TomarDispositivo(5, "BALDM01")).Returns<int, string>((id, cod) => libre);
            repositorioMock.Setup(r => r.LiberarDispositivos(10)).Callback<int>(id =>
            {
                dispositivo.TomadoPor = null;
                libre = true;
                otroOrquestador = null;
            });

            var resultado = target.RecargarConfiguracion("BALDM01");
            procesadorDispMock.Verify(p => p.Dispose(), Times.Once());
            procesadorFactoryMock.Verify(p => p.ProcesadorParaDispositivo(It.IsAny<Dispositivo>(), It.IsAny<Action<IProcesadorDispositivo>>()), Times.Exactly(2));
            orquestadorRemotoMock.Verify(o => o.RecargarConfiguracion("BALDM01"), Times.Once());

            Assert.That(resultado, Is.Not.Null);
            Assert.That(resultado.Mensaje, Is.Not.Null);
            Assert.That(resultado.Mensaje.Codigo, Is.EqualTo(Codigos.OK));

            repositorioMock.Verify(r => r.LiberarDispositivo(It.IsAny<int>(), It.IsAny<string>()), Times.Once());
            repositorioMock.Verify(r => r.LiberarDispositivos(10), Times.Once());
        }

        [Test]
        public void TestListarLectores()
        {
            var dispositivos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigLectorTarjetas, bool>>>(),
                        It.IsAny<Expression<Func<ConfigLectorTarjetas, DispositivoDto>>>())).Returns(dispositivos);

            var resultado = target.ListarLectores();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigLectorTarjetas, bool>>>(),
                        It.IsAny<Expression<Func<ConfigLectorTarjetas, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarBalanzas()
        {
            var balanzas = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigCabezal, bool>>>(),
                        It.IsAny<Expression<Func<ConfigCabezal, DispositivoDto>>>())).Returns(balanzas);

            var resultado = target.ListarBalanzas();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigCabezal, bool>>>(),
                        It.IsAny<Expression<Func<ConfigCabezal, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarCamaras()
        {
            var camaras = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigCamara, bool>>>(),
                        It.IsAny<Expression<Func<ConfigCamara, DispositivoDto>>>())).Returns(camaras);

            var resultado = target.ListarCamaras();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigCamara, bool>>>(),
                        It.IsAny<Expression<Func<ConfigCamara, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarHumedimetros()
        {
            var humedimetros = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>(),
                        It.IsAny<Expression<Func<ConfigHumedimetro, DispositivoDto>>>())).Returns(humedimetros);

            var resultado = target.ListarHumedimetros();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigHumedimetro, bool>>>(),
                        It.IsAny<Expression<Func<ConfigHumedimetro, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarSensores()
        {
            var sensores = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(),
                        It.IsAny<Expression<Func<ConfigSensor, DispositivoDto>>>())).Returns(sensores);

            var resultado = target.ListarSensores();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigSensor, bool>>>(),
                        It.IsAny<Expression<Func<ConfigSensor, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarDataOffLine()
        {
            var dispositivos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigJsonFromIotBox, bool>>>(),
                        It.IsAny<Expression<Func<ConfigJsonFromIotBox, DispositivoDto>>>())).Returns(dispositivos);

            var resultado = target.ListarJsonFromIotBox();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigJsonFromIotBox, bool>>>(),
                        It.IsAny<Expression<Func<ConfigJsonFromIotBox, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarBarrerasSemaforos()
        {
            var barrerasSemaforos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigBarrera, bool>>>(),
                        It.IsAny<Expression<Func<ConfigBarrera, DispositivoDto>>>())).Returns(barrerasSemaforos);

            var resultado = target.ListarBarrerasSemaforos();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigBarrera, bool>>>(),
                       It.IsAny<Expression<Func<ConfigBarrera, DispositivoDto>>>()), Times.Once());
        }

        [Test]
        public void TestListarPuestoDeViandas()
        {
            var dispositivos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigPuestoDeVianda, bool>>>(),
                        It.IsAny<Expression<Func<ConfigPuestoDeVianda, DispositivoDto>>>())).Returns(dispositivos);

            var resultado = target.ListarPuestoDeViandas();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigPuestoDeVianda, bool>>>(),
                        It.IsAny<Expression<Func<ConfigPuestoDeVianda, DispositivoDto>>>()), Times.Once());
        }
        [Test]
        public void TestListarLectoresQr()
        {
            var dispositivos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigLectorQr, bool>>>(),
                        It.IsAny<Expression<Func<ConfigLectorQr, DispositivoDto>>>())).Returns(dispositivos);

            var resultado = target.ListarLectoresQr();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigLectorQr, bool>>>(),
                        It.IsAny<Expression<Func<ConfigLectorQr, DispositivoDto>>>()), Times.Once());
        }
        [Test]
        public void TestListarMolinetes()
        {
            var dispositivos = new List<DispositivoDto>()
            {
                new DispositivoDto()
                {
                    Codigo = "1",
                    Descripcion = "1"
                }
            };
            repositorioFactoryMock.Setup(x => x.Repositorio()).Returns(repositorioMock.Object);
            repositorioMock.Setup(
                x =>
                    x.Listar(It.IsAny<Expression<Func<ConfigMolinete, bool>>>(),
                        It.IsAny<Expression<Func<ConfigMolinete, DispositivoDto>>>())).Returns(dispositivos);

            var resultado = target.ListarMolinetes();
            repositorioFactoryMock.Verify(x => x.Repositorio(), Times.Once());
            repositorioMock.Verify(x => x.Listar(It.IsAny<Expression<Func<ConfigMolinete, bool>>>(),
                        It.IsAny<Expression<Func<ConfigMolinete, DispositivoDto>>>()), Times.Once());
        }
        [Test]
        public void TestSuscribir()
        {
            var comando = new ComandoSuscribir()
            {
                CodigoDispositivo = "Codigo",
                CodigoEvento = "Evento",
                Persistente = true,
                RutaAccesoSuscriptor = "c"
            };
            var resultado = target.Suscribir(comando);

            Assert.That(resultado.IdSuscripcion, Is.EqualTo(0));
        }

        [Test]
        public void TestRecargarConfiguracionDispositivoNoEncontradoException()
        {
            repositorioFactoryMock.Setup(f => f.Repositorio()).Throws(new DispositivoNoEncontradoException());
            var result = target.RecargarConfiguracion("Dispositivo");
            Assert.That(result.Mensaje.Descripcion, Is.EqualTo("El dispositivo Dispositivo no existe o no está habilitado"));
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(100));
        }

        [Test]
        public void TestRecargarConfiguracionDriverNoEncontradoException()
        {
            repositorioFactoryMock.Setup(f => f.Repositorio()).Throws(new DriverNoEncontradoException("Mensaje", null, "Dispositivo", "Driver"));
            var result = target.RecargarConfiguracion("Dispositivo");
            Assert.That(result.Mensaje.Descripcion, Is.EqualTo("No se encontró el driver 'Driver' para el dispositivo Dispositivo"));
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(102));
        }

        [Test]
        public void TestRecargarConfiguracionTipoDispositivoIncorrectoException()
        {
            repositorioFactoryMock.Setup(f => f.Repositorio()).Throws(new TipoDispositivoIncorrectoException("Mensaje", "Dispositivo", "Driver"));
            var result = target.RecargarConfiguracion("Dispositivo");
            Assert.That(result.Mensaje.Descripcion, Is.EqualTo("El tipo de dispositivo 'Driver' no concuerda con el comando que se quiere ejecutar: Dispositivo"));
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(103));
        }

        [Test]
        public void TestRecargarConfiguracionTipoDriverIncorrectoException()
        {
            repositorioFactoryMock.Setup(f => f.Repositorio()).Throws(new TipoDriverIncorrectoException("Mensaje", "Dispositivo", "Driver"));
            var result = target.RecargarConfiguracion("Dispositivo");
            Assert.That(result.Mensaje.Descripcion, Is.EqualTo("El tipo del driver 'Driver' no concuerda con el tipo de dispositivo Dispositivo"));
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(104));
        }

        [Test]
        public void TestRecargarConfiguracionExceptionGenerica()
        {
            repositorioFactoryMock.Setup(f => f.Repositorio()).Throws(new Exception());
            var result = target.RecargarConfiguracion("Dispositivo");
            Assert.That(result.Mensaje.Descripcion, Is.EqualTo("Ocurrió un error al ejecutar el comando"));
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(999));
        }
    }
}
