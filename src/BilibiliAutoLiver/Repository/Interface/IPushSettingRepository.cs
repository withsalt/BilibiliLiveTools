using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Interface
{
    public interface IPushSettingRepository : IBaseRepository<PushSetting, long>, IRepositoryDependency
    {

    }
}
