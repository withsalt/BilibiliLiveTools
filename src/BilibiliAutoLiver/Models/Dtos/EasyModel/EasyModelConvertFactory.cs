using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Services.Interface;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelConvertFactory
    {
        public static async Task ParamsCheck(PushSettingUpdateRequest request, PushSetting setting, IFFMpegService ffmpegService)
        {
            IEasyModelParamsConvertor convertor = null;
            switch (request.InputType)
            {
                case InputType.Video:
                    convertor = new EasyModelVideoParamsConvertor(ffmpegService, setting);
                    break;
                case InputType.Desktop:
                    convertor = new EasyModelDesktopParamsConvertor(ffmpegService, setting);
                    break;
                case InputType.Camera:
                case InputType.CameraPlus:
                    convertor = new EasyModelCameraParamsConvertor(ffmpegService, setting);
                    break;
            }
            if (convertor == null)
            {
                throw new Exception("未知的推流类型");
            }
            await convertor.ParamsCheck(request);
        }
    }
}
