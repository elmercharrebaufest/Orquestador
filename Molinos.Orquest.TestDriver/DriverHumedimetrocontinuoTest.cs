using System.Collections.Generic;
using System.Threading;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Molinos.Orquest.Test.Mocks;
using NUnit.Framework;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    class DriverHumedimetrocontinuoTest
    {
        private string codigoHumedimetro = "HUMEM01";
        private ConfigHumedimetro configHumedimetro;

        [SetUp]
        public void SetUp()
        {
            configHumedimetro = new ConfigHumedimetro
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverHumedimetroContinuo, Molinos.Orquest.DriversImpl",
                DireccionIp = "127.0.0.1",//"192.168.34.13",
                Puerto = 1470,
                ComandoHumedad = null,
                LongFrase = 110,
                TimeoutLectura = 10000,
                DelimitadorCampos = ",",
                PosicionCampoHumedad = 3
            };
        }

        [Test]
        public void TestObtenerHumedadEmulador()
        {
            // Precondición: emulador puesto en función "HUMEDIMETRO"
            // Luego deiniciar el test, presionar el boton debajo del texto "HUMED" en el emulador
            var driver = new DriverHumedimetroContinuo();
            driver.Log = new NullLogger();
            driver.Inicializar(codigoHumedimetro, configHumedimetro);
            var humedad = new List<decimal?>();
            for (int i = 0; i < 1000; i++)
            {
                humedad.Add(driver.ObtenerHumedad());
                Thread.Sleep(1000);
            }
            Assert.That(driver.ObtenerHumedad(), Is.EqualTo(12.5M));
        }

        [Test]
        public void TestNoConecta()
        {
            //Precondición: conversor RS232-Ethernet apagado
            var driver = new DriverHumedimetro();
            driver.Inicializar(codigoHumedimetro, configHumedimetro);

            Assert.That(() => driver.ObtenerHumedad(), Throws.InstanceOf<ConexionDispositivoDriverException>());
        }

        [Test]
        public void TestNoResponde()
        {
            //Precondición: conversor RS232-Ethernet encendido, emulador apagado
            var driver = new DriverHumedimetro();
            driver.Inicializar(codigoHumedimetro, configHumedimetro);

            Assert.That(() => driver.ObtenerHumedad(), Throws.InstanceOf<ConexionDispositivoDriverException>());
        }
    }
}
