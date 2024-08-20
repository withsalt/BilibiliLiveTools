using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Bilibili.AspNetCore.Apis.Interface;

namespace Bilibili.AspNetCore.Apis.Services.Lock
{
    public class LocalLockService : ILocalLockService
    {
        private ConcurrentDictionary<string, DateTime> Container { get; } = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentDictionary<string, bool> SpanLockFlags { get; } = new ConcurrentDictionary<string, bool>();

        private readonly static object _lock = new object();

        public LocalLockService()
        {

        }

        public bool IsLocked(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            if (Container.TryGetValue(key, out DateTime last) && last >= DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }

        public bool Lock(string key, TimeSpan expired)
        {
            if (expired.TotalMilliseconds <= 0)
            {
                throw new ArgumentException("过期时间不能小于或等于0");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            lock (_lock)
            {
                var expTime = DateTime.UtcNow.AddSeconds(expired.TotalSeconds);
                if (DateTime.UtcNow > expTime)
                {
                    return false;
                }
                if (Container.TryGetValue(key, out DateTime last) && last >= DateTime.UtcNow)
                {
                    return false;
                }
                Container[key] = expTime;
                return true;
            }
        }

        public bool SpinLock(string key, int timeoutSeconds)
        {
            if (timeoutSeconds < 1)
            {
                throw new ArgumentException("过期时间不能小于或等于0");
            }
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            bool isLocked = false;

            int timeWaited = 0;
            const int waitInterval = 50;
            int timeoutMilliseconds = timeoutSeconds * 1000;
            Stopwatch sw = Stopwatch.StartNew();

            while (sw.ElapsedMilliseconds < timeoutMilliseconds && !isLocked)
            {
                isLocked = SpanLockFlags.TryAdd(key, true);
                if (isLocked)
                {
                    break;
                }
                Thread.Sleep(waitInterval);
                timeWaited += waitInterval;
            }
            sw.Stop();
            return isLocked;
        }


        public bool UnLock(string key, bool isSpan = false)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException("key");
            }
            if (isSpan)
            {
                return SpanLockFlags.TryRemove(key, out _);
            }
            else
            {
                lock (_lock)
                {
                    return Container.TryRemove(key, out _);
                }
            }
        }
    }
}
