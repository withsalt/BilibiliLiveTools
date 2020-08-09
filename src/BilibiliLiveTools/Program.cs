using Bilibili;
using Bilibili.Api;
using Bilibili.Helper;
using Bilibili.Model.Live.LiveRoomStreamInfo;
using Bilibili.Model.Live.StartLiveInfo;
using Bilibili.Settings;
using BilibiliLiveTools.Common;
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
        private static Users _users;

        static async Task Main(string[] args)
        {
            GlobalSettings.Logger = Logger.Instance;
            if (!BitConverter.IsLittleEndian)
            {
                GlobalSettings.Logger.LogWarning("在BigEndian模式的CPU下工作可能导致程序出错");
                GlobalSettings.Logger.LogWarning("如果出现错误，请创建issue");
            }
            string usersConfigPath = Path.Combine(Environment.CurrentDirectory, "Settings", "Users.json");
            try
            {
                GlobalSettings.LoadAll();
                _users = Users.FromJson(File.ReadAllText(usersConfigPath));
                if (_users == null || _users.Count == 0)
                {
                    throw new Exception("Load user info failed.");
                }
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogException(ex);
                GlobalSettings.Logger.LogError($"缺失或无效配置文件，请检查是否添加\"{usersConfigPath}\"");
                return;
            }
            LoginApiExtensions.LoginDataUpdated += (sender, e) => File.WriteAllText(usersConfigPath, _users.ToJson());
            User user = _users[0];
            if (!await user.Login())
            {
                GlobalSettings.Logger.LogError($"账号{user.Account}登录失败！");
                return;
            }
            //加载配置文件
            LiveSetting liveSetting = LoadLiveSettingConfig();
            if (liveSetting == null)
            {
                GlobalSettings.Logger.LogError($"加载直播设置配置文件失败！");
                return;
            }
            //获取直播间信息
            LiveRoomStreamDataInfo roomInfo = await LiveApi.GetRoomInfo(user);
            if (roomInfo == null)
            {
                GlobalSettings.Logger.LogError($"开启直播失败，无法获取直播间信息！");
                return;
            }

            //开始使用ffmpeg推流直播
            await StartPublish(user, liveSetting, $"{roomInfo.Rtmp.Addr}{roomInfo.Rtmp.Code}");

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
        /// 开始推流
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static async Task<bool> StartPublish(User user, LiveSetting setting, string url)
        {
            try
            {
                if (string.IsNullOrEmpty(setting.CmdString))
                {
                    throw new Exception("CMD string can not null.");
                }
                if (setting.CmdString.IndexOf("[[URL]]") < 0)
                {
                    throw new Exception("Cmd args cannot find '[[URL]]' mark.");
                }
                setting.CmdString = setting.CmdString.Replace("[[URL]]", $"\"{url}\"");
                int firstNullChar = setting.CmdString.IndexOf((char)32);
                if (firstNullChar < 0)
                {
                    throw new Exception("Cannot find cmd process name(look like 'ping 127.0.0.1','ping' is process name).");
                }
                string cmdName = setting.CmdString.Substring(0, firstNullChar);
                string cmdArgs = setting.CmdString.Substring(firstNullChar);
                if (string.IsNullOrEmpty(cmdArgs))
                {
                    throw new Exception("Cmd args cannot null.");
                }
                var psi = new ProcessStartInfo
                {
                    FileName = cmdName,
                    Arguments = cmdArgs,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                bool isAutoRestart = true;
                while (isAutoRestart)
                {
                    isAutoRestart = setting.AutoRestart;
                    while (!await NetworkTools.NetworkCheck())
                    {
                        GlobalSettings.Logger.LogInfo($"Wait for network...");
                        await Task.Delay(3000);
                    }
                    if (!await LiveRoomStateCheck(user))
                    {
                        GlobalSettings.Logger.LogInfo($"Start live failed...");
                        return false;
                    }
                    StartLiveDataInfo liveInfo = await StartLive(user, setting);
                    if (liveInfo == null)
                    {
                        GlobalSettings.Logger.LogInfo($"Start live failed...");
                        return false;
                    }
                    //启动
                    var proc = Process.Start(psi);
                    if (proc == null)
                    {
                        throw new Exception("Can not exec set cmd.");
                    }
                    else
                    {
                        //开始读取命令输出
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
                        //退出检测
                        if (!Console.IsInputRedirected)
                        {
                            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                            {
                                isAutoRestart = false;
                            };
                        }
                        if (isAutoRestart)
                        {
                            GlobalSettings.Logger.LogInfo($"Cmd exited. Auto restart.");
                        }
                        else
                        {
                            GlobalSettings.Logger.LogInfo($"Cmd exited.");
                        }
                    }
                    if (isAutoRestart)
                    {
                        GlobalSettings.Logger.LogInfo($"Wait for restart...");
                        //如果开启了自动重试，那么等待60s后再次尝试
                        await Task.Delay(60000);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static async Task<StartLiveDataInfo> StartLive(User user, LiveSetting liveSetting)
        {
            try
            {
                //先停止历史直播
                //if (await LiveApi.StopLive(user))
                //{
                //    GlobalSettings.Logger.LogInfo("尝试关闭历史直播...成功！");
                //}
                //修改直播间名称
                if (await LiveApi.UpdateLiveRoomName(user, liveSetting.LiveRoomName))
                {
                    GlobalSettings.Logger.LogInfo($"成功修改直播间名称，直播间名称：{liveSetting.LiveRoomName}");
                }
                //更新分类
                if (await LiveApi.UpdateLiveCategory(user, liveSetting.LiveCategory))
                {
                    GlobalSettings.Logger.LogInfo($"成功修改直播间分类，直播间分类ID：{liveSetting.LiveCategory}");
                }
                StartLiveDataInfo liveInfo = await LiveApi.StartLive(user, liveSetting.LiveCategory);
                if (liveInfo != null)
                {
                    GlobalSettings.Logger.LogInfo("开启直播成功！");
                    GlobalSettings.Logger.LogInfo($"我的直播间地址：http://live.bilibili.com/{liveInfo.RoomId}");
                    GlobalSettings.Logger.LogInfo($"推流地址：{liveInfo.Rtmp.Addr}{liveInfo.Rtmp.Code}");
                }
                return liveInfo;
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError($"开启直播失败！错误：{ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 直播间状态检查
        /// </summary>
        /// <returns></returns>
        private static async Task<bool> LiveRoomStateCheck(User user)
        {
            try
            {
                string info = await LoginApi.GetInfoAsync(user);
                if (string.IsNullOrEmpty(info))
                {
                    if (!await user.Login())
                    {
                        throw new Exception("User login failed. exit.");
                    }
                }
                LiveRoomStreamDataInfo roomInfo = await LiveApi.GetRoomInfo(user);
                if (roomInfo == null)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 测试FFmpeg
        /// </summary>
        /// <returns></returns>
        private static bool FFmpegTest()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"ffmpeg",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                using var proc = Process.Start(psi);
                if (proc != null && proc.Id > 0)
                {
                    proc.Kill();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
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
