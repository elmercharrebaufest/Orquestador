using System.Globalization;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.Dominio.Helpers;
using Molinos.Orquest.DriversImpl;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Dominio
{
    [TestFixture]
    public class ExtensionesSerializacionTest
    {
        [TestCase("{\"nombre\":\"valor\"}", true)]
        [TestCase("{'key1' : 'value1' }", true)]
        [TestCase("{ 'key1' : {  key2 : value2 } }", false)]
        [TestCase("esto no es un json", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        [TestCase("[]", true)]
        [TestCase("[}", false)]
        public void TestIsValidJson(string json, bool esperado)
        {
            var resultado = ExtensionesSerializacion.IsValidJson(json);
            Assert.AreEqual(esperado, resultado);
        }
    }
}
