using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Dominio.Resultados;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using Ninject.Extensions.Logging;
using NUnit.Framework;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    class DriverPantallaTest
    {
        private string codigoPantalla = "PAN1";
        private ConfigPantalla configPantalla;
        private DriverPantalla driver;
        [SetUp]
        public void SetUp()
        {
            configPantalla = new ConfigPantalla
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverPantalla, Molinos.Orquest.DriversImpl",
                UrlFTP = "ftp://localhost/GraficoCamionesPorHora.png",
                UrlSCATO = "http://via.placeholder.com/150",
                TiempoDeRefresco = 10
            };

        }

        [Test]
        public void TestObtenerUltimaFotoError()
        {
            driver = new DriverPantalla();
            driver.Log = new NullLogger();
           
            var result = driver.ObtenerUltimaFoto();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Mensaje, Is.Not.Null);
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(Codigos.ErrorDriver));
        }

        [Test]
        public void TestObtenerUltimaFoto()
        {
            driver = new DriverPantalla();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoPantalla, configPantalla);

            var result = driver.ObtenerUltimaFoto();
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Mensaje, Is.Not.Null);
            Assert.That(result.Mensaje.Codigo, Is.EqualTo(Codigos.OK));
        }

    }
}
