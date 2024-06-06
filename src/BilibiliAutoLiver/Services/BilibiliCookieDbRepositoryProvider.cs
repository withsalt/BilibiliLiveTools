using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Exceptions;
using Bilibili.AspNetCore.Apis.Providers;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.Extensions.DependencyInjection;

namespace BilibiliAutoLiver.Services
{
    public class BilibiliCookieDbRepositoryProvider : IBilibiliCookieRepositoryProvider
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly object _locker = new object();


        public BilibiliCookieDbRepositoryProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public Task Delete()
        {
            lock (_locker)
            {
                ICookieSettingRepository repository = GetCookieSettingRepository();
                List<CookieSetting> allCookies = repository.Where(p => !p.IsDeleted).ToList();
                if (allCookies?.Any() != true)
                {
                    return Task.CompletedTask;
                }
                repository.Delete(allCookies);
                return Task.CompletedTask;
            }
        }

        public async Task<string> Read()
        {
            ICookieSettingRepository repository = GetCookieSettingRepository();
            CookieSetting cookieSetting = await repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();
            if (cookieSetting == null)
            {
                throw new CookieException("没有已保存的Cookie，请先登录");
            }
            if (string.IsNullOrWhiteSpace(cookieSetting.Content))
            {
                throw new CookieException("'Cookie内容为空");
            }
            return cookieSetting.Content;
        }

        public Task Write(string cookie)
        {
            lock (_locker)
            {
                ICookieSettingRepository repository = GetCookieSettingRepository();
                CookieSetting cookieSetting = repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).First();
                if (cookieSetting == null)
                {
                    cookieSetting = new CookieSetting()
                    {
                        CreatedTime = DateTime.UtcNow,
                        CreatedUserId = GlobalConfigConstant.SYS_USERID,
                    };
                }

                cookieSetting.Content = cookie;
                cookieSetting.UpdatedTime = DateTime.UtcNow;
                cookieSetting.UpdatedUserId = GlobalConfigConstant.SYS_USERID;

                repository.InsertOrUpdate(cookieSetting);
                return Task.CompletedTask;
            }
        }

        private ICookieSettingRepository GetCookieSettingRepository()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                ICookieSettingRepository repository = scope.ServiceProvider.GetRequiredService<ICookieSettingRepository>();
                return repository;
            }
        }
    }
}
