using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public interface IEasyModelParamsConvertor
    {
        public Task ParamsCheck(PushSettingUpdateRequest request);
    }
}
