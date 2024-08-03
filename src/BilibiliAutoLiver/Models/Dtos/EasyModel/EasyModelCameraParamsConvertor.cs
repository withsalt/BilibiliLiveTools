using System;
using System.Linq;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelCameraParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelCameraParamsConvertor(PushSetting setting) : base(setting)
        {

        }

        protected override void Check(PushSettingUpdateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InputDeviceName))
            {
                throw new Exception("输入设备不能为空");
            }
            if (!ResolutionHelper.TryParse(request.InputDeviceResolution, out int width, out int height))
            {
                throw new Exception("输入分辨率不能为空或参数错误");
            }
            if (request.InputDeviceAudioFrom && string.IsNullOrWhiteSpace(request.InputDeviceAudioDeviceName))
            {
                throw new Exception("当选择推流音频来源于设备时，音频设备不能为空");
            }
            this.Setting.DeviceName = request.InputDeviceName;
            this.Setting.InputResolution = request.InputDeviceResolution;
            this.Setting.InputAudioSource = request.InputDeviceAudioFrom ? InputAudioSource.Device : InputAudioSource.File;
            this.Setting.AudioId = !request.InputDeviceAudioFrom && request.InputDeviceAudioId.HasValue && request.InputDeviceAudioId.Value > 0 ? request.InputDeviceAudioId.Value : null;
            this.Setting.AudioDevice = request.InputDeviceAudioFrom ? request.InputDeviceAudioDeviceName : "";

            var plugins = request.InputDevicePlugins?.Trim('\r', '\n', ' ')?.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (plugins != null)
            {
                this.Setting.Plugins = string.Join(',', plugins.Select(p => p.Trim('\r', '\n', ' ')));
            }
            else
            {
                this.Setting.Plugins = null;
            }
        }
    }
}
