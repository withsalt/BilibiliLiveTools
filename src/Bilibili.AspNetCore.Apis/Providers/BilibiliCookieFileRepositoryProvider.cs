using System;
using System.IO;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Exceptions;

namespace Bilibili.AspNetCore.Apis.Providers
{
    public class BilibiliCookieFileRepositoryProvider : IBilibiliCookieRepositoryProvider
    {

        private static readonly object _locker = new object();

        private string _cookiePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cookies.json");

        public BilibiliCookieFileRepositoryProvider()
        {

        }

        public Task Delete()
        {
            lock (_locker)
            {
                if (File.Exists(_cookiePath))
                {
                    File.Delete(_cookiePath);
                }
                return Task.CompletedTask;
            }
        }

        public async Task<string> Read()
        {
            if (!File.Exists(_cookiePath))
            {
                throw new CookieException("File 'cookie.json' not fount.");
            }
            string result = (await File.ReadAllTextAsync(_cookiePath))?.Trim('\r', '\n', ' ');
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new CookieException("'Cookie内容为空");
            }
            return result;
        }

        public Task Write(string cookie)
        {
            lock (_locker)
            {
                File.WriteAllText(_cookiePath, cookie);
                return Task.CompletedTask;
            }
        }
    }
}
