using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelCameraParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelCameraParamsConvertor(IFFMpegService ffmpegService, PushSetting setting) : base(ffmpegService, setting)
        {

        }

        protected override async Task Check(PushSettingUpdateRequest request)
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
            if (request.InputDeviceFramerate <= 0)
            {
                throw new Exception("输入帧数不能为空或参数错误");
            }
            (string format, string deviceName) = CommonHelper.GetDeviceFormatAndName(request.InputDeviceName);
            List<string> supportResolutions = await FFMpegService.ListVideoDeviceSupportResolutions(deviceName);
            if (supportResolutions?.Any() != true)
            {
                throw new Exception($"视频输入设备{request.InputDeviceName}未获取到受支持的输入分辨率");
            }
            if (!supportResolutions.Any(p => ResolutionHelper.Equal(p, request.InputDeviceResolution)))
            {
                throw new Exception($"视频输入设备{request.InputDeviceName}不支持输入分辨率：{request.InputDeviceResolution}，受支持的最大分辨率：{supportResolutions.Last()}");
            }

            this.Setting.DeviceName = request.InputDeviceName;
            this.Setting.InputResolution = request.InputDeviceResolution;
            this.Setting.InputFramerate = request.InputDeviceFramerate;
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
