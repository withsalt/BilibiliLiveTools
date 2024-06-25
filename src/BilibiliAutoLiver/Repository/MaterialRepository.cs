using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository.Base;
using BilibiliAutoLiver.Repository.Interface;
using DJT.Data.Model.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Repository
{
    public class MaterialRepository : BaseUnitOfWorkRepository<Material, long>, IMaterialRepository
    {
        private readonly ILogger<MaterialRepository> _logger;
        private readonly IFreeSql _db;
        private readonly AppSettings _appSettings;

        public MaterialRepository(BaseUnitOfWorkManager uow
            , ILogger<MaterialRepository> logger
            , IOptions<AppSettings> settingOptions) : base(uow)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _db = Orm;
            _appSettings = settingOptions?.Value ?? throw new ArgumentNullException(nameof(settingOptions));
        }

        public async Task<QuaryPageModel<MaterialDto>> GetPageAsync(MaterialListPageRequest param)
        {
            try
            {
                Expression<Func<Material, bool>> whereExp = c => c.IsDeleted == false;
                Expression<Func<Material, object>> orderExp = c => c.Id;
                OrderByType orderByType = OrderByType.Asc;

                #region 查询字段

                if (!string.IsNullOrEmpty(param.FileName) && param.FileType == FileType.Unknow)
                {
                    whereExp = c => c.IsDeleted == false && c.Name.Contains(param.FileName);
                }
                else if (string.IsNullOrEmpty(param.FileName) && param.FileType != FileType.Unknow)
                {
                    whereExp = c => c.IsDeleted == false && c.FileType == param.FileType;
                }
                else if (!string.IsNullOrEmpty(param.FileName) && param.FileType != FileType.Unknow)
                {
                    whereExp = c => c.IsDeleted == false && c.Name.Contains(param.FileName) && c.FileType == param.FileType;
                }

                #endregion

                #region 排序

                if (string.IsNullOrEmpty(param.Order))
                    param.Order = "desc";
                if (string.IsNullOrEmpty(param.Field))
                    param.Field = "id";
                if (param.Order.Equals("asc", StringComparison.CurrentCultureIgnoreCase))
                    orderByType = OrderByType.Asc;
                else
                    orderByType = OrderByType.Desc;

                orderExp = (param.Field.ToLower()) switch
                {
                    "name" => c => c.Name,
                    "size" => c => c.Size,
                    "path" => c => c.Path,
                    "filetype" => c => c.FileType,
                    "createdtime" => c => c.CreatedTime,
                    _ => c => c.Id,
                };
                #endregion

                var total = await _db.Select<Material>().CountAsync();
                var selector = _db.Select<Material>()
                    .Where(whereExp)
                    .Page(param.Page, param.Limit);

                if (orderByType == OrderByType.Asc)
                {
                    selector.OrderBy(orderExp);
                }
                else
                {
                    selector.OrderByDescending(orderExp);
                }

                List<Material> list = await selector.ToListAsync();
                List<MaterialDto> listDtos = list?.Select(p => p.ToDto(Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory))).ToList() ?? new List<MaterialDto>();

                return new QuaryPageModel<MaterialDto>()
                {
                    DataCount = total,
                    PageCount = list.Count,
                    Page = param.Page,
                    PageSize = param.Limit,
                    Data = listDtos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Select soup info list by page failed. {ex.Message}", ex);
                return null;
            }
        }
    }
}
