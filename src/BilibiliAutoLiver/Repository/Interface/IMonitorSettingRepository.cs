using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Interface
{
    public interface IMonitorSettingRepository : IBaseRepository<MonitorSetting, long>, IRepositoryDependency
    {
        Task<MonitorSetting> GetCacheAsync();

        void RemoveCache();
    }
}
