using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Molinos.Orquest.Dominio.Comandos;
using Molinos.Orquest.Dominio.Consultas;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Recursos;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Test")]
    [TestFixture]
    public class BalanzaPuertoControllerTest
    {
        private BalanzaPuertoController target;
        private Mock<IRepositorioFactory> repositorioFactoryMock;
        private Mock<IRepositorio> repositorioMock;
        private Mock<IDriverFactory> driverFactoryMock;
        private List<ConfigBalanzaPuerto> balanzas;
        private List<Dispositivo> dispositivos;
        private Mock<IServicioOrquestador> servicioMock;
        private IConversor conversor;

        [SetUp]
        public void SetUp()
        {
            servicioMock = new Mock<IServicioOrquestador>();
            conversor = FactoryConversor.ConversorAutoMapper;
            repositorioMock = new Mock<IRepositorio>();
            repositorioFactoryMock = new Mock<IRepositorioFactory>();
            repositorioFactoryMock.Setup(factory => factory.Repositorio()).Returns(repositorioMock.Object);
            driverFactoryMock = new Mock<IDriverFactory>();
            driverFactoryMock.Setup(f => f.DriversDisponibles<IDriverBalanzaPuerto>()).Returns(new List<string> { "DriverCab1", "DriverCab2" });
            repositorioMock.Setup(f => f.Listar(It.IsAny<Expression<Func<Dispositivo, bool>>>())).Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Where(q.Compile()).ToList());
            target = new BalanzaPuertoController(repositorioFactoryMock.Object, driverFactoryMock.Object, conversor,servicioMock.Object, new NullLogger());
            
            balanzas = new List<ConfigBalanzaPuerto>
            {
                new ConfigBalanzaPuerto
                    {
                        Id = 1,
                        ClaseDriver = "Clase1",
                        TimeoutLectura = 800,
                        CantidadCaracteresTotal = 10,
                        CaracterIzquierdaACompletar = "0",
                        ComandoBorrado = "D",
                        ComandoConsulta = "P",
                        DireccionIp = "10.10.104.3",
                        IntervaloPolling= 300,
                        LongFrase = 800,
                        PosDesde = 0,
                        PosHasta = 800,
                        Puerto= 3001,
                        Dispositivo = new Dispositivo{Id = 1, Activo = true, Codigo = "Cod1", Descripcion = "Desc1"}
                    }
            };

            dispositivos = new List<Dispositivo>
                {
                    new Dispositivo{Codigo = "1", Descripcion = "11", EsConcentrador = false, Id = 1},
                    new Dispositivo{Codigo = "2", Descripcion = "22", Id = 2}
                };
        }

        [Test]
        public void TestIndex()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigBalanzaPuerto, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigBalanzaPuerto>(balanzas, 1, 2, 2));
            const string filter = "";
            var result = target.Index(filter) as ViewResult;
            IEnumerable<ConfigBalanzaPuertoModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.Null.Or.Empty);
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestListar()
        {
            repositorioMock.Setup(s => s.Listar(It.IsAny<Expression<Func<ConfigBalanzaPuerto, bool>>>(), It.IsAny<Paginacion>())).Returns(new ListaPaginada<ConfigBalanzaPuerto>(balanzas, 1, 2, 2));
            const string filter = "";
            var result = target.Listar(filter) as ViewResult;
            IEnumerable<ConfigBalanzaPuertoModel> results = target.ViewBag.Items;
            Assert.That(result.ViewName, Is.EqualTo("Listar"));
            Assert.That(results.Select(x => x.Id), Is.EquivalentTo(new List<int> { 1 }));
            Assert.That((object)target.ViewBag.Items.Items[0].Dispositivo.Codigo, Is.EqualTo("Cod1"));
        }

        [Test]
        public void TestCrear()
        {
            var result = target.Crear() as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
        }

        [Test]
        public void TestCrearPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigBalanzaPuerto, bool>>>()))
                    .Returns<Expression<Func<ConfigBalanzaPuerto, bool>>>(q => balanzas.Any((q.Compile())));

            var cabezal = new ConfigBalanzaPuertoModel            
            {
                Id = 0,
                ClaseDriver = "Clase2",
                TimeoutLectura = 800,
                CantidadCaracteresTotal = 10,
                CaracterIzquierdaACompletar = "0",
                ComandoBorrado = "D",
                ComandoConsulta = "P",
                DireccionIp = "10.10.104.3",
                IntervaloPolling= 300,
                LongFrase = 800,
                PosDesde = 0,
                PosHasta = 800,
                Puerto= 3001,
                Dispositivo = new Dispositivo{Id = 0, Activo = true, Codigo = "Cod2", Descripcion = "Desc2" }
            };           

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBalanzaPuerto>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            //server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once()); //con .Never() funciona
        }

        [Test]
        public void TestCrearPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any(q.Compile()));

            var balanza = new ConfigBalanzaPuertoModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                TimeoutLectura = 800,
                CantidadCaracteresTotal = 10,
                CaracterIzquierdaACompletar = "0",
                ComandoBorrado = "D",
                ComandoConsulta = "P",
                DireccionIp = "10.10.104.3",
                IntervaloPolling = 300,
                LongFrase = 800,
                PosDesde = 0,
                PosHasta = 800,
                Puerto = 3001,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Crear(balanza) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBalanzaPuerto>(f => f.Dispositivo.Codigo == balanza.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.Cabezal_CodigoExistente));
        }

        [Test]
        public void TestModificar()
        {
            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigBalanzaPuerto>(It.IsAny<int>())).Returns(balanzas[0]);
            var result = target.Modificar(It.IsAny<int>()) as ViewResult;
            Assert.That(result.View, Is.Null.Or.Empty);
            server.Verify(v => v.UrlEncode(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void TestModificarPost()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<ConfigBalanzaPuerto, bool>>>()))
                    .Returns<Expression<Func<ConfigBalanzaPuerto, bool>>>(q => balanzas.Any((q.Compile())));
            balanzas[0].Dispositivo.Configuracion = balanzas[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(balanzas[0].Dispositivo);

            var cabezal = new ConfigBalanzaPuertoModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                TimeoutLectura = 800,
                CantidadCaracteresTotal = 10,
                CaracterIzquierdaACompletar = "0",
                ComandoBorrado = "D",
                ComandoConsulta = "P",
                DireccionIp = "10.10.104.3",
                IntervaloPolling = 300,
                LongFrase = 800,
                PosDesde = 0,
                PosHasta = 800,
                Puerto = 3001,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(cabezal) as ContentResult;
            var expectedResult = new ContentResult { Content = "ajax-edit-success" };

            Assert.NotNull(result);
            Assert.AreEqual(result.Content, expectedResult.Content);
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            servicioMock.Verify(s => s.RecargarConfiguracion(It.IsAny<string>()), Times.Once());
            //server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Once());
        }

        [Test]
        public void TestModificarPostInvalido()
        {
            repositorioMock.Setup(s => s.Existe(It.IsAny<Expression<Func<Dispositivo, bool>>>()))
                    .Returns<Expression<Func<Dispositivo, bool>>>(q => dispositivos.Any((q.Compile())));
            balanzas[0].Dispositivo.Configuracion = balanzas[0];
            repositorioMock.Setup(s => s.Obtener<Dispositivo>(It.IsAny<int>())).Returns(balanzas[0].Dispositivo);

            var cabezal = new ConfigBalanzaPuertoModel
            {
                Id = 0,
                ClaseDriver = "Clase2",
                TimeoutLectura = 800,
                CantidadCaracteresTotal = 10,
                CaracterIzquierdaACompletar = "0",
                ComandoBorrado = "D",
                ComandoConsulta = "P",
                DireccionIp = "10.10.104.3",
                IntervaloPolling = 300,
                LongFrase = 800,
                PosDesde = 0,
                PosHasta = 800,
                Puerto = 3001,
                Dispositivo = new Dispositivo { Id = 0, Activo = true, Codigo = "1", Descripcion = "Desc2" }
            };

            var server = new Mock<HttpServerUtilityBase>();
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Server).Returns(server.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);

            var result = target.Modificar(cabezal) as ViewResult;

            Assert.NotNull(result);
            Assert.That(result.View, Is.Null.Or.Empty);
            Assert.That(target.ModelState.IsValid, Is.EqualTo(false));
            repositorioMock.Verify(v => v.Agregar(It.Is<ConfigBalanzaPuerto>(f => f.Dispositivo.Codigo == cabezal.Dispositivo.Codigo)), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            server.Verify(v => v.UrlDecode(It.IsAny<string>()), Times.Never());
            Assert.That(target.ModelState.Values.Where(w => w.Errors.Count > 0).SelectMany(s => s.Errors).First().ErrorMessage, Is.EqualTo(Textos.BalanzaPuerto_CodigoExistente));
        }

        [Test]
        public void TestEliminar()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            repositorioMock.Setup(s => s.Obtener<ConfigBalanzaPuerto>(It.IsAny<int>())).Returns(balanzas[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigBalanzaPuerto>()), Times.Once());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Once());
            Assert.That(actual.Content, Is.EqualTo("true"));
        }

        [Test]
        public void TestEliminarOrquestadorTomado()
        {
            var request = new Mock<HttpRequestBase>();
            request.SetupGet(x => x.Headers).Returns(
                new System.Net.WebHeaderCollection
                    {
                        {"X-Requested-With", "XMLHttpRequest"}
                    });
            var context = new Mock<HttpContextBase>();
            context.SetupGet(x => x.Request).Returns(request.Object);
            target.ControllerContext = new ControllerContext(context.Object, new RouteData(), target);
            balanzas[0].Dispositivo.TomadoPor = new Orquestador();
            repositorioMock.Setup(s => s.Obtener<ConfigBalanzaPuerto>(It.IsAny<int>())).Returns(balanzas[0]);
            var actual = target.Eliminar(It.IsAny<int>()) as ContentResult;
            Assert.NotNull(actual);
            repositorioMock.Verify(v => v.Remover(It.IsAny<ConfigBalanzaPuerto>()), Times.Never());
            repositorioMock.Verify(v => v.GuardarCambios(), Times.Never());
            Assert.That(actual.Content, Is.EqualTo(Textos.Error_EliminarCabezalPorOrquestado));
        }

        [Test]
        public void TestProbar()
        {
            repositorioMock.Setup(s => s.Obtener<ConfigBalanzaPuerto>(1)).Returns(balanzas[0]);
            var result = target.Probar(1) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as ConfigBalanzaPuertoModel;
            Assert.That(result.Model, Is.Not.Null);
            Assert.That(model.Id, Is.EqualTo(balanzas[0].Id));
        }

        [Test]
        public void TestConsultaBalanzada()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoConsultaBalanzada
                        {
                            Mensaje = Mensaje.ResultadoOK(),
                            ValoresBalanzada = new Dictionary<string, string>() { { "1" , "ok" } }
                        });

            var result = target.ConsultaBalanzada("1", null) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
        }

        [Test]
        public void TestConsultaBalanzadaErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoConsultaBalanzada
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error"),
                            ValoresBalanzada = new Dictionary<string, string>()
                        });

            var result = target.ConsultaBalanzada("1", null) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(new Mensaje(Codigos.DriverNoEncontrado, "error").ToString()));
        }

        [Test]
        public void TestConsultaBalanzadaErrorServicio()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Throws<Exception>();

            var result = target.ConsultaBalanzada("1", null) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }

        [Test]
        public void TestConsultaBalanzadaPorRango()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzadaPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoConsultaBalanzadaPorRango
                        {
                            Mensaje = Mensaje.ResultadoOK(),
                            Balanzadas = new List<ResultadoConsultaBalanzada>() { new ResultadoConsultaBalanzada()
                            {
                                Mensaje = Mensaje.ResultadoOK()
                            } }
                        });

            var result = target.ConsultaBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
        }

        [Test]
        public void TestConsultaBalanzadaPorRangoErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzadaPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoConsultaBalanzadaPorRango
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error"),
                            Balanzadas = new List<ResultadoConsultaBalanzada>(){ new ResultadoConsultaBalanzada()
                            {
                                Mensaje =new Mensaje(Codigos.DriverNoEncontrado, "error")
                            } }
                        });

            var result = target.ConsultaBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(2));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Contains.Substring(new Mensaje(Codigos.DriverNoEncontrado, "error").ToString()));
        }

        [Test]
        public void TestConsultaBalanzadaPorRangoErrorServicio()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarConsultaBalanzadaPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Throws<Exception>();

            var result = target.ConsultaBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }






        [Test]
        public void TestBorrarBalanzada()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoBorrarBalanzada
                        {
                            Mensaje = Mensaje.ResultadoOK()
                        });

            var result = target.BorradoBalanzada("1", 123) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
        }

        [Test]
        public void TestBorradoBalanzadaErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoBorrarBalanzada
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error")
                        });
      
            var result = target.BorradoBalanzada("1", 123) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(new Mensaje(Codigos.DriverNoEncontrado, "error").ToString()));
        }

        [Test]
        public void TestBorradoBalanzadaErrorServicio()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzada>(cmd => cmd.CodigoDispositivo == "1")))
                        .Throws<Exception>();

            var result = target.BorradoBalanzada("1", 123) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }

        [Test]
        public void TestBorradoBalanzadaPorRango()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzadasPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoBorrarBalanzadasPorRango
                        {
                            Mensaje = Mensaje.ResultadoOK(),
                            IdBalanzadaInicio = 10,
                            IdBalanzadaFin = 12,
                            BalanzadasBorradas = new Dictionary<string, bool>()
                        });

            var result = target.BorradoBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.False);
        }

        [Test]
        public void TestBorradoBalanzadaPorRangoErrorDispositivo()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzadasPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Returns(new ResultadoBorrarBalanzadasPorRango
                        {
                            Mensaje = new Mensaje(Codigos.DriverNoEncontrado, "error")
                        });

            var result = target.BorradoBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Contains.Substring(new Mensaje(Codigos.DriverNoEncontrado, "error").ToString()));
        }

        [Test]
        public void TestBorradoBalanzadaPorRangoErrorServicio()
        {
            servicioMock.Setup(s => s.Ejecutar(It.Is<EjecutarBorrarBalanzadasPorRango>(cmd => cmd.CodigoDispositivo == "1")))
                        .Throws<Exception>();

            var result = target.BorradoBalanzadaPorRango("1", 10, 12) as ViewResult;
            Assert.That(result.View, Is.Null);
            var model = result.Model as IList<ResultadoPruebaModel>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model, Has.Count.EqualTo(1));
            Assert.That(model[0].Error, Is.True);
            Assert.That(model[0].Message, Is.EqualTo(Textos.PruebaItc_ErrorServicio));
        }
    }
}
