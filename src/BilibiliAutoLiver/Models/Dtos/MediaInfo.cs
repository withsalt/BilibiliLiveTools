using BilibiliAutoLiver.Models.Enums;
using FFMpegCore;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class MediaInfo
    {
        public MediaInfo()
        {

        }

        public MediaInfo(IMediaAnalysis mediaAnalysis, FileType fileType)
        {
            if (mediaAnalysis == null) return;

            this.Duration = mediaAnalysis.Duration.TotalSeconds;
            this.FileType = fileType;
            if (mediaAnalysis.PrimaryVideoStream != null)
            {
                this.VideoStream = new VideoStream()
                {
                    Index = mediaAnalysis.PrimaryVideoStream.Index,
                    Duration = mediaAnalysis.PrimaryVideoStream.Duration.TotalSeconds,
                    AvgFrameRate = mediaAnalysis.PrimaryVideoStream.AvgFrameRate,
                    BitsPerRawSample = mediaAnalysis.PrimaryVideoStream.BitsPerRawSample,
                    Profile = mediaAnalysis.PrimaryVideoStream.Profile,
                    Width = mediaAnalysis.PrimaryVideoStream.Width,
                    Height = mediaAnalysis.PrimaryVideoStream.Height,
                    FrameRate = mediaAnalysis.PrimaryVideoStream.FrameRate,
                    PixelFormat = mediaAnalysis.PrimaryVideoStream.PixelFormat,
                    Rotation = mediaAnalysis.PrimaryVideoStream.Rotation,
                };
            }
            if (mediaAnalysis.PrimaryAudioStream != null)
            {
                this.AudioStream = new AudioStream()
                {
                    Index = mediaAnalysis.PrimaryAudioStream.Index,
                    Duration = mediaAnalysis.PrimaryAudioStream.Duration.TotalSeconds,
                    Channels = mediaAnalysis.PrimaryAudioStream.Channels,
                    ChannelLayout = mediaAnalysis.PrimaryAudioStream.ChannelLayout,
                    SampleRateHz = mediaAnalysis.PrimaryAudioStream.SampleRateHz,
                    Profile = mediaAnalysis.PrimaryAudioStream.Profile,
                };
            }

            switch (fileType)
            {
                case FileType.Video:
                    this.PrimaryIndex = mediaAnalysis.PrimaryVideoStream.Index;
                    break;
                case FileType.Music:
                    this.PrimaryIndex = mediaAnalysis.PrimaryAudioStream.Index;
                    break;
            }
        }

        public double Duration { get; set; }

        public int PrimaryIndex { get; set; }

        public FileType FileType { get; set; }

        public AudioStream AudioStream { get; set; }

        public VideoStream VideoStream { get; set; }
    }

    public class AudioStream
    {
        public int Index { get; set; }

        public double Duration { get; set; }

        public int Channels { get; set; }

        public string ChannelLayout { get; set; }

        public int SampleRateHz { get; set; }

        public string Profile { get; set; }
    }

    public class VideoStream
    {
        public int Index { get; set; }

        public double Duration { get; set; }

        public double AvgFrameRate { get; set; }

        public int BitsPerRawSample { get; set; }

        public string Profile { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public double FrameRate { get; set; }

        public string PixelFormat { get; set; }

        public int Rotation { get; set; }
    }
}
