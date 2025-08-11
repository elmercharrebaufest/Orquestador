using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using Moq;
using Ninject.Extensions.Logging;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    class DriverBalanzaPuertoTest
    {
        private DriverBalanzaPuerto driver;

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void TestStart()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA8";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA8", new Regex(@"[^-?\d]").Replace("BZA8", ""));
            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00066114;31-07-2018:52    BZA8STARCOMM:PECASO         BOD.:BODEGA 2       EXP.:MOLINOS AGRO SADES.:ESPANA         BUQ.:KP ALBATROSS   TNW:09000000kgTAW:00000000kg", 8);

            Assert.AreEqual(diccionario["id"], "00066114");
            Assert.AreEqual(diccionario["tipoBalanzada"], "inicio");
            Assert.AreEqual(diccionario["fecha"], "31-07-202518:52");
            Assert.AreEqual(diccionario["numeroBalanza"], "8");
            Assert.AreEqual(diccionario["commodity"], "PECASO");
            Assert.AreEqual(diccionario["bodega"], "BODEGA 2");
            Assert.AreEqual(diccionario["exportador"], "MOLINOS AGRO SA");
            Assert.AreEqual(diccionario["destino"], "ESPANA");
            Assert.AreEqual(diccionario["vapor"], "KP ALBATROSS");
            Assert.AreEqual(diccionario["pesoProgramado"], "09000000");
            Assert.AreEqual(diccionario["toneladasaw"], "00000000");
        }

        [Test]
        public void TestStop()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA7";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA7", new Regex(@"[^-?\d]").Replace("BZA7", ""));
            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00000013;03-08-2016:14    BZA7STARCOMM:SOJA           BOD.:BODEGA 7       EXP.:MOLINOS AGRO   DES.:CHINA          BUQ.:ALBATROSS      TNW:00100000kgTAW:00000000kg", 8);
            Assert.AreEqual(diccionario["id"], "00000013");
            Assert.AreEqual(diccionario["tipoBalanzada"], "inicio");
            Assert.AreEqual(diccionario["fecha"], "03-08-202516:14");
            Assert.AreEqual(diccionario["commodity"], "SOJA");
            Assert.AreEqual(diccionario["bodega"], "BODEGA 7");
            Assert.AreEqual(diccionario["exportador"], "MOLINOS AGRO");
            Assert.AreEqual(diccionario["destino"], "CHINA");
            Assert.AreEqual(diccionario["vapor"], "ALBATROSS");
            Assert.AreEqual(diccionario["pesoProgramado"], "00100000");
            Assert.AreEqual(diccionario["toneladasaw"], "00000000");
            Assert.AreEqual(diccionario["numeroBalanza"], "7");
        }

        [Test]
        public void TestBATR()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA8";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA8", new Regex(@"[^-?\d]").Replace("BZA8", ""));

            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00069018;03-08-2001:25    BZA8BATR G:+07550kg  T:+00000kg TAW:00007550kgCAP:   377t/h", 8);
            Assert.AreEqual(diccionario["id"], "00069018");
            Assert.AreEqual(diccionario["tipoBalanzada"], "balanzada");
            Assert.AreEqual(diccionario["fecha"], "03-08-202501:25");
            Assert.AreEqual(diccionario["numeroBalanza"], "8");
            Assert.AreEqual(diccionario["pesoBruto"], "07550");
            Assert.AreEqual(diccionario["pesoTara"], "00000");
            Assert.AreEqual(diccionario["pesoNeto"], "7550");
            Assert.AreEqual(diccionario["toneladasaw"], "00007550");
            Assert.AreEqual(diccionario["capacidad"], "377t/h");

        }

        [Test]
        public void TestBATRM()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA7";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA7", new Regex(@"[^-?\d]").Replace("BZA7", ""));

            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00092537;24-09-2011:28    BZA7BATRMG:+17700kg MT:+00010kg TAW:00302560kgCAP:   343t/h", 8);
            Assert.AreEqual(diccionario["id"], "00092537");
            Assert.AreEqual(diccionario["tipoBalanzada"], "balanzada");
            Assert.AreEqual(diccionario["fecha"], "24-09-202511:28");
            Assert.AreEqual(diccionario["numeroBalanza"], "7");
            Assert.AreEqual(diccionario["pesoBruto"], "17700");
            Assert.AreEqual(diccionario["pesoTara"], "00010");
            Assert.AreEqual(diccionario["pesoNeto"], "17690");
            Assert.AreEqual(diccionario["toneladasaw"], "00302560");
            Assert.AreEqual(diccionario["capacidad"], "343t/h");

        }

        [Test]
        public void TestErr()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA8";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA8", new Regex(@"[^-?\d]").Replace("BZA8", ""));

            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00069049;03-08-2020 01:50      BZA8 ERRR      WRN 70:REACTION FEED GATE", 8);
            Assert.AreEqual(diccionario["id"], "00069049");
            Assert.AreEqual(diccionario["tipoBalanzada"], "error");
            Assert.AreEqual(diccionario["fecha"], "03-08-202001:50");
            Assert.AreEqual(diccionario["numeroBalanza"], "8");

        }

        [Test]
        public void TestStart2()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA7";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA7", new Regex(@"[^-?\d]").Replace("BZA7", ""));

            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00000025;03-08-2016:15    BZA7STARCOMM:SOJA           BOD.:BODEGA 7       EXP.:MOLINOS AGRO   DES.:CHINA          BUQ.:ALTST2BATROSS  TNW:00100000kgTAW:00000000kg", 25);
            Assert.AreEqual(diccionario["id"], "00000025");
            Assert.AreEqual(diccionario["tipoBalanzada"], "inicio");
            Assert.AreEqual(diccionario["fecha"], "03-08-202516:15");
            Assert.AreEqual(diccionario["commodity"], "SOJA");
            Assert.AreEqual(diccionario["bodega"], "BODEGA 7");
            Assert.AreEqual(diccionario["exportador"], "MOLINOS AGRO");
            Assert.AreEqual(diccionario["destino"], "CHINA");
            Assert.AreEqual(diccionario["vapor"], "ALTST2BATROSS");
            Assert.AreEqual(diccionario["pesoProgramado"], "00100000");
            Assert.AreEqual(diccionario["toneladasaw"], "00000000");
            Assert.AreEqual(diccionario["numeroBalanza"], "7");

        }

        [Test]
        public void TestConvertirInicioI410ABS()
        {
            var consulta = "00000001;1   BALANZA 7;27/06/25;16:06:43;           2;           1;               CHILE;            ;      200000;              0;           1;                SOJA;EXPEDICION;   63;    0;1:";
            var res = DriverBalanzaPuerto.I410ABSParser.ConvertirADictionary(consulta);

            Assert.AreEqual(res["id"], "00000001");
            Assert.AreEqual(res["tipoBalanzada"], "inicio");
            Assert.AreEqual(res["numeroBalanza"], "7");
            Assert.AreEqual(res["fecha"], "27-06-202516:06");
            Assert.AreEqual(res["bodega"], "2");
            Assert.AreEqual(res["vapor"], "1");
            Assert.AreEqual(res["destino"], "CHILE");
            Assert.AreEqual(res["exportador"], "");
            Assert.AreEqual(res["pesoProgramado"], "200000");
            Assert.AreEqual(res["toneladasaw"], "0");
            Assert.AreEqual(res["commodity"], "SOJA");
        }

        [Test]
        public void TestConvertirFinalI410ABS()
        {
            var consulta = "00000018;2   BALANZA 7;27/06/25;16:07;27/06/25;16:06;     00:01;   868;           2;           1;               CHILE;            ;           1;                SOJA;EXPEDICION;   63;    0;      200000;          14470;11";
            var res = DriverBalanzaPuerto.I410ABSParser.ConvertirADictionary(consulta);
            Assert.AreEqual(res["id"], "00000018");
            Assert.AreEqual(res["tipoBalanzada"], "fin");
            Assert.AreEqual(res["numeroBalanza"], "7");
            Assert.AreEqual(res["fecha"], "27-06-202516:07");
            Assert.AreEqual(res["bodega"], "2");
            Assert.AreEqual(res["vapor"], "1");
            Assert.AreEqual(res["destino"], "CHILE");
            Assert.AreEqual(res["exportador"], "");
            Assert.AreEqual(res["commodity"], "SOJA");
            Assert.AreEqual(res["pesoProgramado"], "200000");
            Assert.AreEqual(res["toneladasaw"], "14470");
        }

        [Test]
        public void TestConvertirBalanzadaI410ABS()
        {
            var consulta = "00000014;3   BALANZA 7;27/06/25;16:07:04;         0;    2910;       0;     0;           2910;6=";
            var res = DriverBalanzaPuerto.I410ABSParser.ConvertirADictionary(consulta);
            Assert.AreEqual(res["id"], "00000014");
            Assert.AreEqual(res["tipoBalanzada"], "balanzada");
            Assert.AreEqual(res["numeroBalanza"], "7");
            Assert.AreEqual(res["fecha"], "27-06-202516:07");
            Assert.AreEqual(res["pesoBruto"], "2910");
            Assert.AreEqual(res["pesoTara"], "0");
            Assert.AreEqual(res["pesoNeto"], "2910");
            Assert.AreEqual(res["capacidad"], "0");
            Assert.AreEqual(res["toneladasaw"], "2910");
        }

        [Test]
        public void TestConvertirErrorI410ABS()
        {
            var consulta = "00000020;4   BALANZA 7;27/06/25;16:10:19;         5;        14;          ERROR ESTABILISACION;2<";
            var res = DriverBalanzaPuerto.I410ABSParser.ConvertirADictionary(consulta);
            Assert.AreEqual(res["id"], "00000020");
            Assert.AreEqual(res["tipoBalanzada"], "error");
            Assert.AreEqual(res["numeroBalanza"], "7");
            Assert.AreEqual(res["fecha"], "27-06-202516:10");
        }

        [Test]
        public void TestConsultaBalanzada()
        {
            // Arrange
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA8";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA8", new Regex(@"[^-?\d]").Replace("BZA8", ""));
            driver.Log = new NullLogger();

            var mockTcpClient = new Mock<ITcpCommandClient>();
            mockTcpClient.Setup(c => c.EnviarComando(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns("00066114;31-07-2018:52    BZA8STARCOMM:PECASO         BOD.:BODEGA 2       EXP.:MOLINOS AGRO SADES.:ESPANA         BUQ.:KP ALBATROSS   TNW:09000000kgTAW:00000000kg");
            driver.Cliente = mockTcpClient.Object;

            var mockConfig = new ConfigBalanzaPuerto
            {
                DireccionIp = "127.0.0.1",
                Puerto = 1234,
                LongFrase = 1024,
                TimeoutLectura = 5000,
                ComandoConsulta = "P",
                CantidadCaracteresTotal = 8,
                CaracterIzquierdaACompletar = "0",
                IntervaloPolling = 1000,
                Dispositivo = new Dispositivo { Codigo = driver.codigoBalanzaPuerto }
            };
            driver.SetAsyncProcessEnabled(false);
            driver.Inicializar(driver.codigoBalanzaPuerto, mockConfig);

            // Act
            var result = driver.ConsultaBalanzada(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(result["id"], "00066114");
            Assert.AreEqual(result["tipoBalanzada"], "inicio");
            Assert.AreEqual(result["fecha"], "31-07-202518:52");
            Assert.AreEqual(result["numeroBalanza"], "8");
            Assert.AreEqual(result["commodity"], "PECASO");
            Assert.AreEqual(result["bodega"], "BODEGA 2");
            Assert.AreEqual(result["exportador"], "MOLINOS AGRO SA");
            Assert.AreEqual(result["destino"], "ESPANA");
            Assert.AreEqual(result["vapor"], "KP ALBATROSS");
            Assert.AreEqual(result["pesoProgramado"], "09000000");
            Assert.AreEqual(result["toneladasaw"], "00000000");
        }

        [Test]
        public void TestConsultaBalanzadaWhenEnviarComandoReturnsNull()
        {
            // Arrange
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA8";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA8", new Regex(@"[^-?\d]").Replace("BZA8", ""));
            driver.Log = new NullLogger();

            var mockTcpClient = new Mock<ITcpCommandClient>();
            mockTcpClient.Setup(c => c.EnviarComando(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns((string)null);
            driver.Cliente = mockTcpClient.Object;

            var mockConfig = new ConfigBalanzaPuerto
            {
                DireccionIp = "127.0.0.1",
                Puerto = 1234,
                LongFrase = 1024,
                TimeoutLectura = 5000,
                ComandoConsulta = "P",
                CantidadCaracteresTotal = 8,
                CaracterIzquierdaACompletar = "0",
                IntervaloPolling = 1000,
                Dispositivo = new Dispositivo { Codigo = driver.codigoBalanzaPuerto }
            };
            driver.SetAsyncProcessEnabled(false);
            driver.Inicializar(driver.codigoBalanzaPuerto, mockConfig);

            // Act & Assert
            Assert.Throws<DriverException>(() => driver.ConsultaBalanzada(null));

            //var result = driver.ConsultaBalanzada(null);
            //Assert.That(result, Is.Not.Null);
            //Assert.True(result.Count == 0, "Expected empty dictionary when EnviarComando returns null.");
        }
    }
}
