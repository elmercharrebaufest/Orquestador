using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using NUnit.Framework;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    class DriverHumedimetroTest
    {
        private string codigoHumedimetro = "HUMEM01";
        private ConfigHumedimetro configHumedimetro;

        [SetUp]
        public void SetUp()
        {
            configHumedimetro = new ConfigHumedimetro
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverHumedimetro, Molinos.Orquest.DriversImpl",
                DireccionIp = "192.168.34.13",
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
            var driver = new DriverHumedimetro();
            driver.Inicializar(codigoHumedimetro, configHumedimetro);

            for (int i = 0; i < 1000; i++)
            {
                Assert.That(driver.ObtenerHumedad(), Is.EqualTo(2M));
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
