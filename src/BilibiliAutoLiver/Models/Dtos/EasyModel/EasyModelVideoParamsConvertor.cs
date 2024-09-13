using System;
using System.Threading.Tasks;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Services.Interface;

namespace BilibiliAutoLiver.Models.Dtos.EasyModel
{
    public class EasyModelVideoParamsConvertor : BaseEasyModelParamsConvertor
    {
        public EasyModelVideoParamsConvertor(IFFMpegService ffmpegService, PushSetting setting) : base(ffmpegService, setting)
        {

        }

        protected override Task Check(PushSettingUpdateRequest request)
        {
            if (request.VideoId <= 0)
            {
                throw new Exception($"请选择推流视频");
            }
            this.Setting.VideoId = request.VideoId;
            this.Setting.AudioId = request.AudioId.HasValue && request.AudioId.Value > 0 ? request.AudioId.Value : null;
            this.Setting.IsMute = request.IsMute;

            if (request.Material.IsDemuxConcat && this.Setting.AudioId > 0)
            {
                throw new Exception($"选择一个视频集合时，不能包含音频");
            }

            return Task.CompletedTask;
        }
    }
}
