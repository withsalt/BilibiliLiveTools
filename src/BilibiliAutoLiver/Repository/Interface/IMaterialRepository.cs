using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Interface
{
    public interface IMaterialRepository : IBaseRepository<Material, long>, IRepositoryDependency
    {
        Task<QuaryPageModel<MaterialDto>> GetPageAsync(MaterialListPageRequest param);
    }
}
