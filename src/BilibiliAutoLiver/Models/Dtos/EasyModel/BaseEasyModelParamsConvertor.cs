using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public abstract class BaseEasyModelParamsConvertor
    {
        public PushSetting Setting { get; }

        public BaseEasyModelParamsConvertor(PushSetting setting)
        {
            this.Setting = setting;
        }

        public abstract void ToEntity(PushSettingUpdateRequest request);
    }
}
