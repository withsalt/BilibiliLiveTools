using Bilibili;
using Bilibili.Api;
using Bilibili.Helper;
using Bilibili.Model.Live.StartLiveInfo;
using Bilibili.Settings;
using BilibiliLiveTools.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace BilibiliLiveTools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string usersFilePath;
            Users users;

            GlobalSettings.Logger = Logger.Instance;
            if (!BitConverter.IsLittleEndian)
            {
                GlobalSettings.Logger.LogWarning("在BigEndian模式的CPU下工作可能导致程序出错");
                GlobalSettings.Logger.LogWarning("如果出现错误，请创建issue");
            }
            usersFilePath = Path.Combine(Environment.CurrentDirectory, "Settings", "Users.json");
            try
            {
                GlobalSettings.LoadAll();
                users = Users.FromJson(File.ReadAllText(usersFilePath));
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogException(ex);
                GlobalSettings.Logger.LogError($"缺失或无效配置文件，请检查是否添加\"{usersFilePath}\"");
                return;
            }
            LoginApiExtensions.LoginDataUpdated += (sender, e) => File.WriteAllText(usersFilePath, users.ToJson());
            User user = users[0];
            if (!await user.Login())
            {
                GlobalSettings.Logger.LogError($"账号{user.Account}登录失败！");
                return;
            }
            if (!await StartLive(user))
            {
                return;
            }

            if (Console.IsInputRedirected || Console.IsOutputRedirected)
            {
                while (true)
                {
                    Thread.Sleep(300);
                }
            }
            else
            {
                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static async Task<bool> StartLive(User user)
        {
            try
            {
                //加载配置文件
                LiveSetting liveSetting = LoadLiveSettingConfig();

                //先停止历史直播
                if (await LiveApi.StopLive(user))
                {
                    GlobalSettings.Logger.LogInfo("尝试关闭历史直播...成功！");
                }
                //修改直播间名称
                if (await LiveApi.UpdateLiveRoomName(user, liveSetting.LiveRoomName))
                {
                    GlobalSettings.Logger.LogInfo($"成功修改直播间名称。直播间名称：{liveSetting.LiveRoomName}");
                }
                StartLiveDataInfo liveInfo = await LiveApi.StartLive(user, liveSetting.LiveCategory);
                if (liveInfo != null)
                {
                    GlobalSettings.Logger.LogInfo("开启直播成功！");
                    GlobalSettings.Logger.LogInfo($"我的直播间地址：http://live.bilibili.com/{liveInfo.RoomId}");
                    GlobalSettings.Logger.LogInfo($"推流地址：{liveInfo.Rtmp.Addr}{liveInfo.Rtmp.Code}");
                }
                //开始使用ffmpeg推流直播
                StartPublish(liveSetting, $"{liveInfo.Rtmp.Addr}{liveInfo.Rtmp.Code}");
                return true;
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError($"开启直播失败！错误：{ex.Message}");
                return false;
            }
        }

        private static bool StartPublish(LiveSetting setting, string url)
        {
            try
            {
                string ffmpegArgs;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    ffmpegArgs = $"-i \"{setting.VideoSource}\" -s {setting.Resolution} -r 30 -input_format mjpeg -c:v h264_omx -vcodec h264 -an -f flv \"{url}\"";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    ffmpegArgs = $"-f dshow -i video=\"{setting.VideoSource}\" -s {setting.Resolution} -r 30 -vcodec libx264 -acodec copy -preset:v ultrafast -tune:v zerolatency -f flv \"{url}\"";
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
                            GlobalSettings.Logger.LogInfo(sr.ReadLine());
                        }

                        if (!proc.HasExited)
                        {
                            proc.Kill();
                        }
                    }
                    GlobalSettings.Logger.LogInfo($"FFmpeg exited.");
                }
                return true;
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError(ex.Message);
                return false;
            }
        }

        private static string GetTitle()
        {
            string productName = GetAssemblyAttribute<AssemblyProductAttribute>().Product;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return $"{productName} v{version}";
        }

        private static T GetAssemblyAttribute<T>()
        {
            return (T)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false)[0];
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
