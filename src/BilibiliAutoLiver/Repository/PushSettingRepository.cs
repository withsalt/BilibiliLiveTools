using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Repository
{
    public class PushSettingRepository : BaseUnitOfWorkRepository<PushSetting, long>, IPushSettingRepository
    {
        private readonly ILogger<PushSettingRepository> _logger;
        private readonly IFreeSql _db;

        public PushSettingRepository(BaseUnitOfWorkManager uow
            , ILogger<PushSettingRepository> logger) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
        }
    }
}
