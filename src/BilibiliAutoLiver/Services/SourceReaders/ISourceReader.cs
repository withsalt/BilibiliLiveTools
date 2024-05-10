using FFMpegCore;

namespace BilibiliAutoLiver.Services.SourceReaders
{
    public interface ISourceReader
    {
        FFMpegArguments BuildInputArg();
        
        /// <summary>
        /// 禁用视频流中的音频
        /// </summary>
        /// <param name="opt"></param>
        void WithDisableAudioChannel(FFMpegArgumentOptions opt);

        /// <summary>
        /// 禁用音频流中的视频
        /// </summary>
        /// <param name="opt"></param>
        void WithDisableVideoChannel(FFMpegArgumentOptions opt);

        /// <summary>
        /// 使用的音频解码器
        /// </summary>
        /// <param name="opt"></param>
        void WithAudioCodec(FFMpegArgumentOptions opt);

        /// <summary>
        /// 使用的视频解码器
        /// </summary>
        /// <param name="opt"></param>
        void WithVideoCodec(FFMpegArgumentOptions opt);
    }
}
