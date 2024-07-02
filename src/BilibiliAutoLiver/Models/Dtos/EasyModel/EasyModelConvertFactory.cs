using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelConvertFactory
    {
        public static void ToEntity(PushSettingUpdateRequest request, PushSetting setting)
        {
            switch (request.InputType)
            {
                case InputType.Video:
                    new EasyModelVideoParamsConvertor(setting).ToEntity(request);
                    break;
                case InputType.Desktop:
                    break;
                case InputType.Camera:
                    break;
                case InputType.CameraPlus:
                    break;
            }
        }
    }
}
