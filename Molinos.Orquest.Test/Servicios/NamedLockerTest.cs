using System.Linq;
using Molinos.Orquest.Servicios.Impl;
using NUnit.Framework;

namespace Molinos.Orquest.Test.Servicios
{
    [TestFixture]
    public class NamedLockerTest
    {
        [Test]
        public void TestGetForSameStringReturnsSameLockObject()
        {
            var target = new NamedLocker();
            var locks = Enumerable.Range(0, 50)
                .AsParallel()
                .Select(x => target.GetLock("MyLock"))
                .ToList();
            var primerLock = locks[0];

            foreach (var lockObject in locks)
            {
                Assert.That(lockObject, Is.SameAs(primerLock));
            }
        }

        [Test]
        public void TestGetForDifferentStringReturnsDifferentLockObject()
        {
            var target = new NamedLocker();
            var locks = Enumerable.Range(0, 50)
                .AsParallel()
                .Select(x => target.GetLock("MyLock" + x))
                .ToList();

            for (var i = 0; i < locks.Count; i++)
            {
                for (var j = 0; j < locks.Count; j++)
                {
                    if (i != j)
                    {
                        Assert.That(locks[i], Is.Not.SameAs(locks[j]));
                    }
                }
            }
        }
    }
}
