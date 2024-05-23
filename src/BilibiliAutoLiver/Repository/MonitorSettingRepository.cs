using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Repository
{
    public class MonitorSettingRepository : BaseUnitOfWorkRepository<MonitorSetting, long>, IMonitorSettingRepository
    {
        private readonly ILogger<MonitorSettingRepository> _logger;
        private readonly IFreeSql _db;
        private readonly IMemoryCache _cache;

        public MonitorSettingRepository(BaseUnitOfWorkManager uow
            , ILogger<MonitorSettingRepository> logger
            , IMemoryCache cache) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

        }

        public async Task<MonitorSetting> GetCacheAsync()
        {
            MonitorSetting setting = await _cache.GetOrCreateAsync(CacheKeyConstant.IS_ENABLED_MONITOR_CACHE_KEY, (p) =>
            {
                p.AbsoluteExpiration = DateTimeOffset.MaxValue;
                return this.Where(p => !p.IsDeleted).FirstAsync();
            });
            return setting;
        }

        public void RemoveCache()
        {
            _cache.Remove(CacheKeyConstant.IS_ENABLED_MONITOR_CACHE_KEY);
        }
    }
}
