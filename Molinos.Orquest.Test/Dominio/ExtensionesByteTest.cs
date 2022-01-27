using System.Globalization;
using Molinos.Orquest.Dominio;
using Molinos.Orquest.DriversImpl;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Dominio
{
    [TestFixture]
    public class ExtensionesByteTest
    {
        [Test]
        public void TestBitAtWithZero()
        {
            var aByte = byte.Parse("00", NumberStyles.HexNumber);
            Assert.That(aByte.BitAt(0), Is.False);
            Assert.That(aByte.BitAt(1), Is.False);
            Assert.That(aByte.BitAt(2), Is.False);
            Assert.That(aByte.BitAt(3), Is.False);
            Assert.That(aByte.BitAt(4), Is.False);
            Assert.That(aByte.BitAt(5), Is.False);
            Assert.That(aByte.BitAt(6), Is.False);
            Assert.That(aByte.BitAt(7), Is.False);
        }

        [Test]
        public void TestBitAt0A()
        {
            var aByte = byte.Parse("0A", NumberStyles.HexNumber);
            Assert.That(aByte.BitAt(0), Is.False);
            Assert.That(aByte.BitAt(1), Is.False);
            Assert.That(aByte.BitAt(2), Is.False);
            Assert.That(aByte.BitAt(3), Is.False);
            Assert.That(aByte.BitAt(4), Is.True);
            Assert.That(aByte.BitAt(5), Is.False);
            Assert.That(aByte.BitAt(6), Is.True);
            Assert.That(aByte.BitAt(7), Is.False);
        }

        [Test]
        public void TestBitAtA0()
        {
            var aByte = byte.Parse("A0", NumberStyles.HexNumber);
            Assert.That(aByte.BitAt(0), Is.True);
            Assert.That(aByte.BitAt(1), Is.False);
            Assert.That(aByte.BitAt(2), Is.True);
            Assert.That(aByte.BitAt(3), Is.False);
            Assert.That(aByte.BitAt(4), Is.False);
            Assert.That(aByte.BitAt(5), Is.False);
            Assert.That(aByte.BitAt(6), Is.False);
            Assert.That(aByte.BitAt(7), Is.False);
        }
    }
}
