using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelVideoParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelVideoParamsConvertor(PushSetting setting) : base(setting)
        {

        }

        public override void ToEntity(PushSettingUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.OutputResolution))
            {
                throw new Exception($"输出分辨率不能为空");
            }
            if (!ResolutionHelper.TryParse(request.OutputResolution, out int width, out int height))
            {
                throw new Exception($"错误的输入分辨率：{request.OutputResolution}");
            }
            if (request.VideoId <= 0)
            {
                throw new Exception($"请选择推流视频");
            }
            this.Setting.OutputResolution = request.OutputResolution;
            this.Setting.CustumOutputParams = request.CustumOutputParams;
            this.Setting.VideoId = request.VideoId;
            this.Setting.AudioId = request.AudioId.HasValue && request.AudioId.Value > 0 ? request.AudioId.Value : null;
            this.Setting.IsMute = request.IsMute;
        }
    }
}
