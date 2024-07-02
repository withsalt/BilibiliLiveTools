using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public interface IEasyModelParamsConvertor
    {
        public void ToEntity(PushSettingUpdateRequest request, PushSetting setting);
    }
}
