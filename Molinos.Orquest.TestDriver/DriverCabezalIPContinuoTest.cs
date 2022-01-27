using System;
using Molinos.Orquest.Dominio.Entidades;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using NUnit.Framework;

namespace Molinos.Orquest.TestDriver
{
    [TestFixture]
    public class DriverCabezalIPContinuoTest
    {
        private string codigoCabezal;
        private ConfigCabezal configCabezal;

        [SetUp]
        public void SetUp()
        {
            codigoCabezal = "BALEM01";
            configCabezal = new ConfigCabezal
            {
                ClaseDriver = "Molinos.Orquest.DriversImpl.DriverCabezalIPContinuo, Molinos.Orquest.DriversImpl",
                DireccionIp = "192.168.34.152",
                Puerto = 1470,
                PosDesde = 5,
                PosHasta = 10,
                CarInicioFrase = "\u0002",
                LongFrase = 16,
                ComandoPeso = "P",
                ComandoCereo = "Z",
                CantLecPesoEstable = 3,
                MaxCantLecPesoEstable = 20,
                IntLecPesoEstable = 10,
                IntLecCereo = 10,
                TimeoutLectura = 5000
            };
        }
        
        [Test]  
        public void TestObtenerPesoEmulador()
        {
            //Precondición: emulador puesto en función "PESO"

            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);
            for (int i = 0; i < 50; i++)
            {
                Assert.That(driver.ObtenerPeso(), Is.EqualTo(31500M));
                Console.Out.WriteLine(DateTime.Now  + " - Peso obtenido " + i);
            }
        }

        [Test]
        public void TestObtenerPesoEmuladorONull()
        {
            //Precondición: emulador puesto en función "PESO"

            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);
            for (int i = 0; i < 50; i++)
            {
                Assert.That(driver.ObtenerPeso(), Is.Null.Or.EqualTo(31500M));
                Console.Out.WriteLine(DateTime.Now + " - Peso obtenido " + i);
            }
        }

        [Test]
        public void TestObtenerPesoEmuladorConDecimales()
        {
            //Precondición: emulador puesto en función "PESO"
            configCabezal.DigitosDecimales = 2;
            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(driver.ObtenerPeso(), Is.EqualTo(315.00M));
        }

        [Test]
        public void TestForzarCeroEmuladorNoVuelveACero()
        {
            //Precondición: emulador puesto en función "PESO"

            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(driver.ForzarCero(), Is.False);
        }

        [Test]
        public void TestObtenerPesoEnCeroEmulador()
        {
            //Precondición: emulador puesto en función "ZERO"

            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(driver.ObtenerPeso(), Is.EqualTo(0M));
        }

        [Test]
        public void TestForzarCeroEmulador()
        {
            //Precondición: emulador puesto en función "ZERO"

            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(driver.ForzarCero(), Is.True);
        }


        [Test]
        public void TestNoConecta()
        {
            //Precondición: conversor RS232-Ethernet apagado
            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(() => driver.ObtenerPeso(), Throws.InstanceOf<ConexionDispositivoDriverException>());
            Assert.That(() => driver.ForzarCero(), Throws.InstanceOf<ConexionDispositivoDriverException>());
        }

        [Test]
        public void TestNoResponde()
        {
            //Precondición: conversor RS232-Ethernet encendido, emulador apagado
            var driver = new DriverCabezalIPContinuo();
            driver.Inicializar(codigoCabezal, configCabezal);

            Assert.That(() => driver.ObtenerPeso(), Throws.InstanceOf<ConexionDispositivoDriverException>());
            Assert.That(() => driver.ForzarCero(), Throws.InstanceOf<ConexionDispositivoDriverException>());
        }
    }
}
