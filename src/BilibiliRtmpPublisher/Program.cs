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
            string serverAddress = "rtmp://10.10.10.150/living/video";
            LiveSetting liveSetting = LoadLiveSettingConfig();

            string cmd;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                cmd = $" -i \"{liveSetting.VideoSource}\" -s {liveSetting.Resolution} -r 30 -input_format mjpeg -c:v h264_omx -vcodec h264 -an -f flv \"{serverAddress}\"";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                cmd = $"-f dshow -i video=\"{liveSetting.VideoSource}\" -s {liveSetting.Resolution} -r 30 -vcodec libx264 -acodec copy -preset:v ultrafast -tune:v zerolatency -f flv \"{serverAddress}\"";
            }
            else
            {
                throw new Exception("UnSupport system.");
            }

            //创建一个ProcessStartInfo对象 使用系统shell 指定命令和参数 设置标准输出
            var psi = new ProcessStartInfo("ffmpeg", cmd) { RedirectStandardOutput = true };
            //启动
            var proc = Process.Start(psi);
            if (proc == null)
            {
                Console.WriteLine("Can not exec.");
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
