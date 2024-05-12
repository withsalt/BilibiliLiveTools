using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Services.Base
{
    public abstract class BasePushStreamService : IPushStreamService
    {
        private readonly ILogger _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly LiveSettings _liveSetting;

        public BasePushStreamService(ILogger logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IOptions<LiveSettings> liveSettingOptions
            , IFFMpegService ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _liveSetting = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
        }

        public abstract Task Start();

        public abstract Task Stop();

        /// <summary>
        /// 测试FFmpeg
        /// </summary>
        /// <returns></returns>
        public async Task CheckFFmpegBinary()
        {
            try
            {
                _logger.LogInformation($"当前ffmpeg路径：{_ffmpeg.GetBinaryPath()}");
                var version = await _ffmpeg.GetVersion();
                if (string.IsNullOrEmpty(version.Version))
                {
                    throw new Exception("获取ffmpeg版本失败。");
                }
                _logger.LogInformation($"当前ffmpeg版本：{version.Version}");
            }
            catch (Exception ex)
            {
                _logger.ThrowLogError($"FFmpeg测试失败，{ex.Message}");
            }
        }

        /// <summary>
        /// 检查配置文件
        /// </summary>
        public Task CheckLiveSetting()
        {
            if (_liveSetting.LiveAreaId <= 0)
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveAreaId填写错误！");
            }
            if (string.IsNullOrWhiteSpace(_liveSetting.LiveRoomName))
            {
                _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.LiveRoomName不能为空！");
            }
            if (_liveSetting.V1?.IsEnabled != true && _liveSetting.V2?.IsEnabled != true)
            {
                _logger.ThrowLogError("V1和V2两种推流方式，至少启用一种！");
            }
            if (_liveSetting.V2?.IsEnabled == true)
            {
                if (_liveSetting.V2.Input == null)
                {
                    _logger.ThrowLogError("配置文件appsettings.json中，需要配置LiveSetting.V2.Input输入源！");
                }
            }
            if (_liveSetting.V2?.IsEnabled != true && _liveSetting.V1?.IsEnabled == true)
            {
                string cmd = _liveSetting.V1?.FFmpegCommands?.GetTargetOSPlatformCommand();
                if (string.IsNullOrWhiteSpace(cmd))
                {
                    _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.V1.FFmpegCommands不能为空！");
                }
                int markIndex = cmd.IndexOf("[[URL]]");
                if (markIndex < 5)
                {
                    _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.V1.FFmpegCommands不正确，命令中未找到 '[[URL]]'标记。");
                }
                if (cmd[markIndex - 1] == '\"')
                {
                    _logger.ThrowLogError("配置文件appsettings.json中，LiveSetting.V1.FFmpegCommands不正确， '[[URL]]'标记前后无需“\"”。");
                }
            }
            return Task.CompletedTask;
        }


        /// <summary>
        /// 检查直播间信息
        /// </summary>
        public async Task CheckLiveRoom()
        {
            //登录
            var userInfo = await _account.LoginByCookie();
            if (userInfo == null || !userInfo.IsLogin)
            {
                _logger.ThrowLogError("登录失败，Cookie无效或已过期，请重新配置Cookie！");
            }
            _logger.LogInformation($"用户{userInfo.Uname}，登录成功！");
            //获取直播间信息
            var liveRoomInfo = await _api.GetMyLiveRoomInfo();
            if (liveRoomInfo == null)
            {
                _logger.ThrowLogError("获取直播间信息失败！");
            }
            if (liveRoomInfo.room_id == 0 || liveRoomInfo.have_live == 0)
            {
                _logger.ThrowLogError("当前用户未开通直播间！");
            }
            _logger.LogInformation($"获取直播间信息成功，当前直播间地址：https://live.bilibili.com/{liveRoomInfo.room_id}，名称：{liveRoomInfo.title}，分区：{liveRoomInfo.parent_name}·{liveRoomInfo.area_v2_name}，直播状态：{(liveRoomInfo.live_status == 1 ? "直播中" : "未开启")}");
            //检查名称
            if (liveRoomInfo.title != _liveSetting.LiveRoomName)
            {
                bool result = await _api.UpdateLiveRoomName(liveRoomInfo.room_id, _liveSetting.LiveRoomName);
                if (!result)
                {
                    _logger.ThrowLogError($"修改直播间名称为【{_liveSetting.LiveRoomName}】失败！");
                }
                _logger.LogInformation($"修改直播间名称为：{_liveSetting.LiveRoomName}，成功！");
                await Task.Delay(1000);
            }
            //检查分区
            if (liveRoomInfo.area_v2_id != _liveSetting.LiveAreaId)
            {
                bool result = await _api.UpdateLiveRoomArea(liveRoomInfo.room_id, _liveSetting.LiveAreaId);
                if (!result)
                {
                    _logger.ThrowLogError($"修改直播间分区为【{_liveSetting.LiveAreaId}】失败！");
                }
                _logger.LogInformation($"修改直播间分区为：{_liveSetting.LiveAreaId}，成功！");
                await Task.Delay(1000);
            }
        }

        protected async Task Delay(CancellationTokenSource tokenSource)
        {
            _logger.LogWarning($"等待{_liveSetting.RetryDelay}s后重新推流...");
            await Task.Delay(_liveSetting.RetryDelay * 1000, tokenSource.Token);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
