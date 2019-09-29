using Bilibili.Helper;
using BilibiliRtmpPublisher.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace BilibiliRtmpPublisher
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverAddress = "rtmp://live.geeiot.net/living/video";
            LiveSetting liveSetting = LoadLiveSettingConfig();

            try
            {
                string ffmpegArgs;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    ffmpegArgs = $"-i \"{liveSetting.VideoSource}\" -s {liveSetting.Resolution} -r 30 -input_format mjpeg -c:v h264_omx -vcodec h264 -an -f flv \"{serverAddress}\"";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ffmpegArgs = $"-f dshow -i video=\"{liveSetting.VideoSource}\" -s {liveSetting.Resolution} -r 30 -vcodec libx264 -acodec copy -preset:v ultrafast -tune:v zerolatency -f flv \"{serverAddress}\"";
                }
                else
                {
                    throw new Exception("UnSupport system.");
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                //启动
                var proc = Process.Start(psi);
                if (proc == null)
                {
                    throw new Exception("Can not exec ffmpeg cmd.");
                }
                else
                {
                    //开始读取
                    using (var sr = proc.StandardOutput)
                    {
                        while (!sr.EndOfStream)
                        {
                            Console.WriteLine(sr.ReadLine());
                        }

                        if (!proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    Console.WriteLine($"FFmpeg exited.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey(false);
        }

        /// <summary>
        /// 自动从文件加载配置文件
        /// </summary>
        /// <param name="configFileName">配置文件名称，位于程序根目录，默认使用appsettings.json</param>
        /// <returns></returns>
        private static LiveSetting LoadLiveSettingConfig()
        {
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "Settings", "LiveSetting.json");
                if (!File.Exists(configPath))
                {
                    throw new Exception("File 'LiveSetting.json' not exist.");
                }
                string loadConfigString = File.ReadAllText(configPath);
                if (string.IsNullOrEmpty(loadConfigString))
                {
                    throw new Exception("Read file 'LiveSetting.json' failed.");
                }
                LiveSetting config = JsonHelper.DeserializeJsonToObject<LiveSetting>(loadConfigString);
                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"Load config failed. {ex.Message}");
            }
        }
    }
}
