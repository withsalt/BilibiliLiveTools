using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Base;
using DJT.Data.Model.Common;
using FreeSql;

namespace BilibiliAutoLiver.Repository.Interface
{
    public interface IMaterialRepository : IBaseRepository<Material, long>, IRepositoryDependency
    {
        Task<QuaryPageModel<MaterialDto>> GetPageAsync(MaterialListPageRequest param);
    }
}
