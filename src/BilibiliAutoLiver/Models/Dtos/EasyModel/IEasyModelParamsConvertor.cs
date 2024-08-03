using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public interface IEasyModelParamsConvertor
    {
        public void ParamsCheck(PushSettingUpdateRequest request);
    }
}
