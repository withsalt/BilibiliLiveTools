using System;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;

        public BasePushStreamService(ILogger logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IServiceProvider serviceProvider
            , IFFMpegService ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public abstract Task Start();

        public abstract Task Stop();

        /// <summary>
        /// 获取推送设置
        /// </summary>
        /// <returns></returns>
        public async Task<SettingDto> GetSetting()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var pushSetting = await scope.ServiceProvider.GetRequiredService<IPushSettingRepository>().Where(p => !p.IsDeleted).FirstAsync();
                var liveSetting = await scope.ServiceProvider.GetRequiredService<ILiveSettingRepository>().Where(p => !p.IsDeleted).FirstAsync();
                return new SettingDto()
                {
                    PushSetting = pushSetting,
                    LiveSetting = liveSetting
                };
            }
        }

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
        public async Task CheckLiveSetting()
        {
            var setting = await GetSetting();
            if (setting.LiveSetting == null)
            {
                _logger.ThrowLogError("请先配置直播间信息");
            }
            if (setting.PushSetting == null)
            {
                _logger.ThrowLogError("请先配置推流信息");
            }
            if (!setting.PushSetting.IsUpdate)
            {
                _logger.ThrowLogError("还未配置推流信息，请先完善推流配置");
            }
            if (setting.LiveSetting.AreaId <= 0)
            {
                _logger.ThrowLogError("直播间分区信息未填写或填写错误");
            }
            if (string.IsNullOrWhiteSpace(setting.LiveSetting.RoomName))
            {
                _logger.ThrowLogError("直播间名称未填写");
            }
            if (setting.PushSetting.Model == ConfigModel.Advance)
            {
                if (!CmdAnalyzer.TryParse(setting.PushSetting.FFmpegCommand, out string message, out _))
                {
                    _logger.ThrowLogError(message);
                }
            }
            if (setting.PushSetting.Model == ConfigModel.Easy)
            {
                _logger.ThrowLogError("暂不支持简易模式");
            }
        }


        /// <summary>
        /// 检查直播间信息
        /// </summary>
        public async Task CheckLiveRoom()
        {
            var setting = await GetSetting();
            if (setting.LiveSetting == null)
            {
                _logger.ThrowLogError("请先配置直播间信息");
            }
            //登录
            var userInfo = await _account.GetUserInfo();
            if (userInfo == null || !userInfo.IsLogin)
            {
                _logger.ThrowLogError("登录失败，Cookie无效或已过期，请重新配置Cookie！");
            }
            _logger.LogInformation($"当前直播用户：{userInfo.Uname}（{userInfo.Mid}）");
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
            //检查名称和分区
            if (liveRoomInfo.title != setting.LiveSetting.RoomName || liveRoomInfo.area_v2_id != setting.LiveSetting.AreaId)
            {
                bool result = await _api.UpdateLiveRoomInfo(liveRoomInfo.room_id, setting.LiveSetting.RoomName, setting.LiveSetting.AreaId);
                if (!result)
                {
                    _logger.ThrowLogError($"修改直播间名称为【{setting.LiveSetting.RoomName}】，分区为【{setting.LiveSetting.AreaId}】失败！");
                }
                _logger.LogInformation($"修改直播间名称为【{setting.LiveSetting.RoomName}】，分区为【{setting.LiveSetting.AreaId}】成功！");
            }
        }

        protected async Task Delay(int sec, CancellationTokenSource tokenSource)
        {
            _logger.LogWarning($"等待{sec}s后重新推流...");
            await Task.Delay(sec * 1000, tokenSource.Token);
        }

        public void Dispose()
        {
            this.Stop();
        }
    }
}
