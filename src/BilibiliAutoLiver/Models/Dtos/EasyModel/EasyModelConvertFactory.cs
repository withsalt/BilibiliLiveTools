using System;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelConvertFactory
    {
        public static void ParamsCheck(PushSettingUpdateRequest request, PushSetting setting)
        {
            IEasyModelParamsConvertor convertor = null;
            switch (request.InputType)
            {
                case InputType.Video:
                    convertor = new EasyModelVideoParamsConvertor(setting);
                    break;
                case InputType.Desktop:
                    convertor = new EasyModelDesktopParamsConvertor(setting);
                    break;
                case InputType.Camera:
                case InputType.CameraPlus:
                    convertor = new EasyModelCameraParamsConvertor(setting);
                    break;
            }
            if (convertor == null)
            {
                throw new Exception("未知的推流类型");
            }
            convertor.ParamsCheck(request);
        }
    }
}
