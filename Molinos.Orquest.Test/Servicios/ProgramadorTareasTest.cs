using System;
using System.Configuration;
using System.Threading;
using Molinos.Orquest.Servicios.Impl;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class ProgramadorTareasTest
    {
        [Test]
        public void TestDepurarSuscripciones()
        {
            var target = new ProgramadorTareas();
            ConfigurationManager.AppSettings["TimeoutSuscripciones"] = "3600";
            ConfigurationManager.AppSettings["IntervaloDepuracionSuscripciones"] = "100";
            ConfigurationManager.AppSettings["IntervaloActualizarEstado"] = "0";
            var evt = new AutoResetEvent(false);
            var fecha = DateTime.MinValue;
            target.DepurarSuscripciones += (source, args) =>
                {
                    fecha = args.Vencimiento;
                    evt.Set();
                };
            target.Iniciar();
            evt.WaitOne();
            var vencimientoAprox = DateTime.Now.AddSeconds(-3600);
            Assert.That(fecha, Is.InRange(vencimientoAprox.AddSeconds(-1), vencimientoAprox.AddSeconds(+1)));
        }
    }
}
