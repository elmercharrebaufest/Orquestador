using Molinos.Orquest.Dominio.Dtos;
using Molinos.Orquest.Drivers;
using Molinos.Orquest.DriversImpl;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace Molinos.Orquest.TestDriver
{
	[TestFixture]
	public class DriverIotBoxV2Test
	{
		Mock<IConfiguracionGeneral> configuracionGeneralMock;
		private DriverIotBoxV2 driver;

		[SetUp]
		public void SetUp()
		{
			configuracionGeneralMock = new Mock<IConfiguracionGeneral>();
			driver = new DriverIotBoxV2(configuracionGeneralMock.Object);
		}

		[Test]
		public void TestParseJsonDataWithEmptyInput()
		{
			var result = driver.ParseJsonData("");
			Assert.IsNotNull(result);
			Assert.IsEmpty(result);
		}

		[Test]
		public void TestParseJsonDataWithValidJson()
		{
			string jsonData = "[{\"Numero\":1,\"Dato\":\"Test\"}]";
			var result = driver.ParseJsonData(jsonData);
			Assert.IsNotNull(result);
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(1, result[0].Numero);
			Assert.AreEqual("Test", result[0].Dato);
		}

		[Test]
		public void TestParseJsonDataWithMultipleObjects()
		{
			string jsonData = "[{\"Numero\":1,\"Dato\":\"Test1\"}][{\"Numero\":2,\"Dato\":\"Test2\"}]";
			var result = driver.ParseJsonData(jsonData);
			Assert.IsNotNull(result);
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(1, result[0].Numero);
			Assert.AreEqual("Test1", result[0].Dato);
			Assert.AreEqual(2, result[1].Numero);
			Assert.AreEqual("Test2", result[1].Dato);
		}

		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"12345678\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"12345678\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"12345678\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"12345678\"}]", 4)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}]", 5)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"socketconnected\"}]", 1)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 7,\"Dato\":\"9999999\"}][{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"socketconnected\"}][{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"ping\"}][{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"pingResponse\"}]", 8)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"ping\"}]", 1)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 0,\"Dato\":\"pingResponse\"}]", 1)]
		[TestCase("[{\"Tipo\": \"entrada\",\"Numero\": 52,\"Dato\":\"12345678\"}]", 1)]
		[TestCase("", 0)]
		public void TestParseJsonData(string jsonData, int expectedCount)
		{
			var result = driver.ParseJsonData(jsonData);
			Assert.IsNotNull(result);
			Assert.AreEqual(expectedCount, result.Count);
		}
	}
}
