using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
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

        }

        [Test]
        public void TestStrop()
        {
            driver = new DriverBalanzaPuerto();
            driver.codigoBalanzaPuerto = "BZA7";
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "TST2", "balanzada");
            driver.PalabrasReservadas.Add(driver.codigoBalanzaPuerto + "BATR", "balanzada");
            driver.PalabrasReservadas.Add("BZA7", new Regex(@"[^-?\d]").Replace("BZA7", ""));
            driver.Log = new NullLogger();

            var diccionario = driver.ConvertirADictionary("00000013;03-08-2016:14    BZA7STARCOMM:SOJA           BOD.:BODEGA 7       EXP.:MOLINOS AGRO   DES.:CHINA          BUQ.:ALBATROSS      TNW:00100000kgTAW:00000000kg", 8);

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
        }

    }
}
