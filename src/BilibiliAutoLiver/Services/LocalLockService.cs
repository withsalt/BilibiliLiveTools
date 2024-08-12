using System;
using System.Collections.Concurrent;
using BilibiliAutoLiver.Services.Interface;

namespace BilibiliAutoLiver.Services
{
    public class LocalLockService : ILocalLockService
    {
        private ConcurrentDictionary<string, DateTime> Container { get; } = new ConcurrentDictionary<string, DateTime>();

        private readonly static object _lock = new object();

        public LocalLockService()
        {

        }

        public bool TryLock(string key)
        {
            if (Container.ContainsKey(key))
            {
                DateTime last = Container[key];
                if (last >= DateTime.UtcNow)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Lock(string key, int expired)
        {
            if (expired <= 0)
            {
                throw new ArgumentException("过期时间不能小于或等于0");
            }
            lock (_lock)
            {
                if (Container.ContainsKey(key))
                {
                    DateTime last = Container[key];
                    if (last >= DateTime.UtcNow)
                    {
                        return false;
                    }
                }
                Container[key] = DateTime.UtcNow.AddSeconds(expired);
                return true;
            }
        }

        public bool UnLock(string key)
        {
            lock (_lock)
            {
                if (!Container.ContainsKey(key))
                {
                    return true;
                }
                return Container.TryRemove(key, out _);
            }
        }
    }
}
