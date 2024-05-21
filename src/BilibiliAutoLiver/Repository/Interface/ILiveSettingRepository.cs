using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Interface
{
    public interface ILiveSettingRepository : IBaseRepository<LiveSetting, long>, IRepositoryDependency
    {

    }
}
