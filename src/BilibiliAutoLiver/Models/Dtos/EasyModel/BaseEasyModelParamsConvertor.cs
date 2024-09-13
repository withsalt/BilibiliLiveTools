using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public abstract class BaseEasyModelParamsConvertor : IEasyModelParamsConvertor
    {
        public PushSetting Setting { get; }

        public IFFMpegService FFMpegService { get; }

        public BaseEasyModelParamsConvertor(IFFMpegService ffmpegService, PushSetting setting)
        {
            this.Setting = setting;
            this.FFMpegService = ffmpegService;
        }

        public async Task ParamsCheck(PushSettingUpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.OutputResolution))
            {
                throw new Exception($"输出分辨率不能为空");
            }
            if (!ResolutionHelper.TryParse(request.OutputResolution, out int width, out int height))
            {
                throw new Exception($"错误的输入分辨率：{request.OutputResolution}");
            }
            if (!Enum.IsDefined(typeof(OutputQualityEnum), request.OutputQuality))
            {
                throw new Exception($"不支持的推流质量，参数值：{request.OutputQuality}");
            }
            if (request.OutputQuality == (int)OutputQualityEnum.Original && request.InputType != InputType.Video)
            {
                throw new Exception($"只有推流视频时，才能选择输出质量为原画");
            }
            this.Setting.OutputResolution = request.OutputResolution;
            this.Setting.Quality = (OutputQualityEnum)request.OutputQuality;
            this.Setting.CustumOutputParams = request.CustumOutputParams;
            this.Setting.CustumVideoCodec = request.CustumVideoCodec;
            this.Setting.InputType = request.InputType;

            await Check(request);
        }

        protected abstract Task Check(PushSettingUpdateRequest request);
    }
}
