using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
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
            this.Setting.OutputResolution = request.OutputResolution;
            this.Setting.CustumOutputParams = request.CustumOutputParams;
            this.Setting.InputType = request.InputType;

            await Check(request);
        }

        protected abstract Task Check(PushSettingUpdateRequest request);
    }
}
