using System;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelVideoParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelVideoParamsConvertor(PushSetting setting) : base(setting)
        {

        }

        protected override void Check(PushSettingUpdateRequest request)
        {
            if (request.VideoId <= 0)
            {
                throw new Exception($"请选择推流视频");
            }
            this.Setting.VideoId = request.VideoId;
            this.Setting.AudioId = request.AudioId.HasValue && request.AudioId.Value > 0 ? request.AudioId.Value : null;
            this.Setting.IsMute = request.IsMute;
        }
    }
}
