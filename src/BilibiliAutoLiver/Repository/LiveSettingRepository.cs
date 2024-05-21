using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Repository
{
    public class LiveSettingRepository : BaseUnitOfWorkRepository<LiveSetting, long>, ILiveSettingRepository
    {
        private readonly ILogger<LiveSettingRepository> _logger;
        private readonly IFreeSql _db;

        public LiveSettingRepository(BaseUnitOfWorkManager uow
            , ILogger<LiveSettingRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
        }
    }
}
