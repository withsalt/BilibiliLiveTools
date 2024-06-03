using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Repository
{
    public class CookieSettingRepository : BaseUnitOfWorkRepository<CookieSetting, long>, ICookieSettingRepository
    {
        private readonly ILogger<CookieSettingRepository> _logger;
        private readonly IFreeSql _db;

        public CookieSettingRepository(BaseUnitOfWorkManager uow
            , ILogger<CookieSettingRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
        }
    }
}
