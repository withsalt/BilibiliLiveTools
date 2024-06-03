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
        private readonly ICookieSettingRepository _repository;

        private static readonly object _locker = new object();


        public BilibiliCookieDbRepositoryProvider(IServiceProvider repository)
        {
            _repository = repository.GetService<ICookieSettingRepository>() ?? throw new ArgumentNullException(nameof(repository));
        }

        public Task Delete()
        {
            lock (_locker)
            {
                var allCookies = _repository.Where(p => !p.IsDeleted).ToList();
                _repository.Delete(allCookies);
                return Task.CompletedTask;
            }
        }

        public async Task<string> Read()
        {
            CookieSetting cookieSetting = await _repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();
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
                CookieSetting cookieSetting = _repository.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).First();
                if(cookieSetting == null)
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

                _repository.InsertOrUpdate(cookieSetting);
                return Task.CompletedTask;
            }
        }
    }
}
