using System.Collections.Concurrent;

namespace Molinos.Orquest.Servicios.Impl
{
    public class NamedLocker : INamedLocker
    {
        private readonly ConcurrentDictionary<string, object> lockDict = new ConcurrentDictionary<string, object>();

        //get a lock for use with a lock(){} block
        public object GetLock(string name)
        {
            return lockDict.GetOrAdd(name, s => new object());
        }

    }
}
