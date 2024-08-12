using System.Threading.Tasks;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public interface IEasyModelParamsConvertor
    {
        public Task ParamsCheck(PushSettingUpdateRequest request);
    }
}
