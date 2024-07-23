using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelDesktopParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelDesktopParamsConvertor(PushSetting setting) : base(setting)
        {

        }

        public override void ToEntity(PushSettingUpdateRequest request)
        {
            BaseParamsCheck(request);
            if (string.IsNullOrWhiteSpace(request.InputScreen))
            {
                throw new Exception("推流屏幕范围不能为空");
            }
            if (!ScreenParamsHelper.TryParse(request.InputScreen, out string message, out _))
            {
                throw new Exception(message);
            }
            if (request.DesktopAudioFrom && string.IsNullOrWhiteSpace(request.DesktopAudioDeviceId))
            {
                throw new Exception("当选择推流音频来源于设备时，音频设备不能为空");
            }

            this.Setting.OutputResolution = request.OutputResolution;
            this.Setting.CustumOutputParams = request.CustumOutputParams;
            this.Setting.InputType = request.InputType;

            this.Setting.InputScreen = request.InputScreen;
            this.Setting.InputAudioSource = request.DesktopAudioFrom ? InputAudioSource.Device : InputAudioSource.File;
            this.Setting.AudioId = !request.DesktopAudioFrom && request.AudioId.HasValue && request.AudioId.Value > 0 ? request.AudioId.Value : null;
            this.Setting.AudioDevice = request.DesktopAudioFrom ? request.DesktopAudioDeviceId : "";
        }
    }
}
