using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Settings
{
    public class AppSettings
    {
        public static string Position { get { return "AppSettings"; } }

        private string _dataDirectory = null;

        public string DataDirectory
        {
            get
            {
                return _dataDirectory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DataDirectory), "Data directory can not null");
                }

                string dic = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    dic = dic.Replace('/', Path.DirectorySeparatorChar);
                else
                    dic = dic.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(dic, "{BaseDirectory}", AppContext.BaseDirectory, out string connTemp))
                {
                    dic = connTemp;
                }
                _dataDirectory = dic;
            }
        }

        private string _connectionString = null;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string DbConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DbConnectionString), "Connection string can not null");
                }
                string connStr = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    connStr = connStr.Replace('/', Path.DirectorySeparatorChar);
                else
                    connStr = connStr.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(connStr, "{BaseDirectory}", AppContext.BaseDirectory, out string connTemp))
                {
                    connStr = connTemp;
                }
                _connectionString = connStr;
            }
        }

        /// <summary>
        /// Bilibili AppKey配置，用于获取APP签名
        /// </summary>
        public BilibiliAppKey BilibiliAppKey { get; set; }

        /// <summary>
        /// 高级模式命令解析采用严格模式
        /// </summary>
        public bool AdvanceStrictMode { get; set; }

        /// <summary>
        /// 允许上传文件类型
        /// </summary>
        public List<AllowExtension> AllowExtensions { get; set; }

        /// <summary>
        /// 预设参数
        /// </summary>
        public FFmpegPresetParams FFmpegPresetParams { get; set; }
    }

    public class FFmpegPresetParams
    {
        public static string Position { get { return "FFMpegPresetParams"; } }

        /// <summary>
        /// 获取或设置像素格式
        /// </summary>
        public string PixelFormat { get; set; }

        /// <summary>
        /// 获取或设置是否启用低延迟标志
        /// </summary>
        public bool LowDelayFlags { get; set; }

        /// <summary>
        /// 获取或设置输出格式
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// 获取或设置输出质量配置
        /// </summary>
        public OutputQuality OutputQuality { get; set; }

        /// <summary>
        /// 获取或者设置输入配置质量
        /// </summary>
        public InputQuality InputQuality { get; set; }
    }

    public class InputQuality
    {
        /// <summary>
        /// 视频输入源自定义参数
        /// </summary>
        public string VideoCustomArgument { get; set; }

        /// <summary>
        /// 音频输入源自定义参数
        /// </summary>
        public string AudioCustomArgument { get; set; }
    }

    /// <summary>
    /// 输出质量设置类
    /// </summary>
    public class OutputQuality
    {
        /// <summary>
        /// 获取或设置高质量设置
        /// </summary>
        public QualitySettings High { get; set; }

        /// <summary>
        /// 获取或设置中等质量设置
        /// </summary>
        public QualitySettings Medium { get; set; }

        /// <summary>
        /// 获取或设置低质量设置
        /// </summary>
        public QualitySettings Low { get; set; }
    }

    /// <summary>
    /// 质量设置类
    /// </summary>
    public class QualitySettings
    {
        /// <summary>
        /// 缓冲区大小
        /// </summary>
        public string BufferSize { get; set; }

        /// <summary>
        /// 获取或设置比特率
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// 编码器预设
        /// </summary>
        public string SpeedPreset { get; set; }

        /// <summary>
        /// 获取或设置帧率模式
        /// </summary>
        public string FpsMode { get; set; }

        /// <summary>
        /// 获取或设置是否为零延迟
        /// </summary>
        public bool ZeroLatency { get; set; }

        /// <summary>
        /// 获取或设置关键帧间隔
        /// </summary>
        public int GOP { get; set; }

        /// <summary>
        /// 获取或设置恒定码率因子
        /// </summary>
        public int ConstantRateFactor { get; set; }

        /// <summary>
        /// 额外的自定义参数
        /// </summary>
        public string CustomArgument { get; set; }
    }
}
