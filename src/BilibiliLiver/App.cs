using BiliAccount;
using BiliAccount.Linq;
using BilibiliLiver.Api;
using BilibiliLiver.Config;
using BilibiliLiver.Model.Live;
using BilibiliLiver.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiver
{
    class App
    {
        private readonly ILogger<App> _logger;
        private readonly ConfigManager _config;
        private readonly LiveApi _liveApi;
        private readonly string _dataFilePath = Path.Combine(AppContext.BaseDirectory, "bilibili.dat");
        private Account _account = null;

        public App(ILogger<App> logger
            , ConfigManager config
            , LiveApi liveApi)
        {
            _logger = logger;
            _config = config;
            _liveApi = liveApi ?? throw new ArgumentNullException(nameof(LiveApi));
        }

        public async Task Run(params string[] args)
        {
            //登录
            await LoginToBilibili();
            _logger.LogInformation($"登录成功，登录账号为：{_config.UserSetting.Account}");
            //获取直播间房间信息
            LiveRoomStreamDataInfo liveRoomInfo = await _liveApi.GetRoomInfo(this._account);
            if (liveRoomInfo == null)
            {
                _logger.LogError($"开启直播失败，无法获取直播间信息！");
                return;
            }
            //测试FFmpeg
            if (!await FFmpegTest())
            {
                _logger.LogError($"开启推流失败，未找到FFmpeg，请确认已经安装FFmpeg！");
                return;
            }
            //开始执行ffmpeg推流
            await UseFFmpegLive(liveRoomInfo.Rtmp.Addr + liveRoomInfo.Rtmp.Code);
        }

        #region Private

        /// <summary>
        /// 测试FFmpeg
        /// </summary>
        /// <returns></returns>
        private async Task<bool> FFmpegTest()
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"ffmpeg",
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                using (var proc = Process.Start(psi))
                {
                    if (proc != null && proc.Id > 0)
                    {
                        string result = await proc.StandardOutput.ReadToEndAsync();
                        if (!string.IsNullOrEmpty(result))
                        {
                            string[] allLines = result.Split('\n');
                            string[] versionLine = allLines.Where(p => p.Contains("ffmpeg version", StringComparison.OrdinalIgnoreCase)).ToArray();
                            if (versionLine.Length > 0)
                            {
                                _logger.LogInformation(versionLine[0]);
                            }
                        }
                        proc.Kill();
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 开始推流
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private async Task<bool> UseFFmpegLive(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(_config.LiveSetting.FFmpegCmd))
                {
                    throw new Exception("推流命令不能为空。");
                }
                int markIndex = _config.LiveSetting.FFmpegCmd.IndexOf("[[URL]]");
                if (markIndex < 5)
                {
                    throw new Exception("在填写的命令中未找到 '[[URL]]'标记。");
                }
                if (_config.LiveSetting.FFmpegCmd[markIndex - 1] == '\"')
                {
                    throw new Exception(" '[[URL]]'标记前后无需“\"”。");
                }
                string newCmd = _config.LiveSetting.FFmpegCmd.Replace("[[URL]]", $"\"{url}\"");
                int firstNullChar = newCmd.IndexOf((char)32);
                if (firstNullChar < 0)
                {
                    throw new Exception("无法获取命令执行名称，比如在命令ffmpeg -version中，无法获取ffmpeg。");
                }
                string cmdName = newCmd.Substring(0, firstNullChar);
                string cmdArgs = newCmd.Substring(firstNullChar);
                if (string.IsNullOrEmpty(cmdArgs))
                {
                    throw new Exception("命令参数不能为空！");
                }
                
                var psi = new ProcessStartInfo
                {
                    FileName = cmdName,
                    Arguments = cmdArgs,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                };
                bool isAutoRestart = true;
                while (isAutoRestart)
                {
                    isAutoRestart = _config.LiveSetting.AutoRestart;
                    //check network
                    while (!await NetworkUtil.NetworkCheck())
                    {
                        _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                        await Task.Delay(10000);
                    }
                    //check token is available
                    if (!ByPassword.IsTokenAvailable(_account.AccessToken))
                    {
                        await LoginToBilibili();
                    }
                    //start live
                    StartLiveDataInfo liveInfo = await StartLive();
                    if (liveInfo == null)
                    {
                        _logger.LogError($"开启B站直播间失败，无法获取推流地址。");
                        return false;
                    }
                    _logger.LogInformation("正在初始化推流指令...");
                    //启动
                    using (var proc = Process.Start(psi))
                    {
                        if (proc == null || proc.Id <= 0)
                        {
                            throw new Exception("无法执行指定的推流指令，请检查FFmpegCmd是否填写正确。");
                        }
                        //退出检测
                        if (!Console.IsInputRedirected)
                        {
                            Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs eventArgs) =>
                            {
                                isAutoRestart = false;
                            };
                        }
                        await proc.WaitForExitAsync();
                        if (isAutoRestart)
                        {
                            _logger.LogWarning($"FFmpeg异常退出，将在60秒后重试推流。");
                        }
                        else
                        {
                            _logger.LogWarning($"FFmpeg异常退出。");
                        }
                    }
                    if (isAutoRestart)
                    {
                        _logger.LogWarning($"等待重新推流...");
                        //如果开启了自动重试，那么等待60s后再次尝试
                        await Task.Delay(60000);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <returns></returns>
        private async Task LoginToBilibili()
        {
            async Task WriteLoginDataToFile(Account account)
            {
                if (account == null) throw new Exception("Write to file data can not null.");
                string dataBeforeEncrypt = JsonUtil.SerializeObject(account);
                string dataAfterEncrypt = AES128.AESEncrypt(dataBeforeEncrypt, _config.AppSetting.Key, "40863a4f-7cbe-4be2-bb54-765233c83d25");
                await File.WriteAllBytesAsync(_dataFilePath, Encoding.UTF8.GetBytes(dataAfterEncrypt));
            }

            async Task LoginByPassword()
            {
                Account account = ByPassword.LoginByPassword(_config.UserSetting.Account, _config.UserSetting.Password);
                if (account == null)
                {
                    throw new Exception("Get account info failed.");
                }
                if (account.LoginStatus != Account.LoginStatusEnum.ByPassword)
                {
                    throw new Exception($"登录失败，登录失败原因：{account.LoginStatus}");
                }
                await WriteLoginDataToFile(account);
                _account = account;
            }

            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    await LoginByPassword();
                    return;
                }
                //获取历史登录信息
                string fileStr = await File.ReadAllTextAsync(_dataFilePath);
                if (string.IsNullOrEmpty(fileStr))
                {
                    await LoginByPassword();
                    return;
                }
                try
                {
                    string decodeStr = AES128.AESDecrypt(fileStr, _config.AppSetting.Key, "40863a4f-7cbe-4be2-bb54-765233c83d25");
                    if (string.IsNullOrEmpty(decodeStr))
                    {
                        await LoginByPassword();
                        return;
                    }
                    Account account = JsonUtil.DeserializeJsonToObject<Account>(decodeStr);
                    //判断AccessToken是否有效
                    if (!ByPassword.IsTokenAvailable(account.AccessToken))
                    {
                        await LoginByPassword();
                        return;
                    }
                    //判断AccessToken是否需要续期
                    if (account.Expires_AccessToken != DateTime.MinValue
                        && account.Expires_AccessToken.AddDays(-7) < DateTime.Now)
                    {
                        DateTime? dt = ByPassword.RefreshToken(account.AccessToken, account.RefreshToken);
                        if (dt == null)
                        {
                            await LoginByPassword();
                            return;
                        }
                        account.Expires_AccessToken = dt.Value;
                        //更新
                        await WriteLoginDataToFile(account);
                        _account = account;
                    }
                    else
                    {
                        _account = account;
                        return;
                    }
                }
                catch
                {
                    await LoginByPassword();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Login to bilibili failed, login account is {_config.UserSetting.Account}. {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 开启
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private async Task<StartLiveDataInfo> StartLive()
        {
            try
            {
                //修改直播间名称
                if (await _liveApi.UpdateLiveRoomName(this._account, _config.LiveSetting.LiveRoomName))
                {
                    _logger.LogInformation($"成功修改直播间名称，直播间名称：{_config.LiveSetting.LiveRoomName}");
                }
                //更新分类
                if (await _liveApi.UpdateLiveCategory(this._account, _config.LiveSetting.LiveCategory))
                {
                    _logger.LogInformation($"成功修改直播间分类，直播间分类ID：{_config.LiveSetting.LiveCategory}");
                }
                StartLiveDataInfo liveInfo = await _liveApi.StartLive(this._account, _config.LiveSetting.LiveCategory);
                if (liveInfo == null)
                {
                    throw new Exception("获取直播信息失败！");
                }
                _logger.LogInformation("开启直播成功！");
                _logger.LogInformation($"我的直播间地址：http://live.bilibili.com/{liveInfo.RoomId}");
                _logger.LogInformation($"推流地址：{liveInfo.Rtmp.Addr}{liveInfo.Rtmp.Code}");
                return liveInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"开启直播失败！错误：{ex.Message}", ex);
                return null;
            }
        }

        #endregion

    }
}
