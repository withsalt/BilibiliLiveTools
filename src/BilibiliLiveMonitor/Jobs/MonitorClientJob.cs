using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using BilibiliLiveMonitor.Configs;
using BilibiliLiveMonitor.Services;
using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiveCommon.Config;
using BilibiliLiveCommon.Utils;

namespace BilibiliLiveMonitor.Jobs
{
    [DisallowConcurrentExecution]
    public class MonitorClientJob : IJob
    {
        private readonly ILogger<MonitorClientJob> _logger;
        private readonly IMemoryCache _cache;
        private readonly IEmailNoticeService _email;
        private readonly LiveSettingsNode _liveSettings;
        private readonly IBilibiliLiveApiService _liveApiService;

        public MonitorClientJob(ILogger<MonitorClientJob> logger
            , IMemoryCache cache
            , IEmailNoticeService email
            , IBilibiliLiveApiService liveApiService
            , IOptions<LiveSettingsNode> liveSettingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _email = email ?? throw new ArgumentNullException(nameof(email));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _liveSettings = liveSettingOptions.Value ?? throw new ArgumentNullException(nameof(liveSettingOptions));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                _logger.LogInformation($"开始查询直播间{_liveSettings.RoomId}状态....");
                if (_liveSettings.RoomId <= 0)
                {
                    throw new Exception("获取需要查询的直播房间号失败，请检查配置文件是否正确配置房间号。");
                }
                RoomPlayInfo playInfo = await _liveApiService.GetRoomPlayInfo(_liveSettings.RoomId);
                if (playInfo == null || playInfo.room_id != _liveSettings.RoomId)
                {
                    throw new Exception("获取直播间信息失败。");
                }
                _logger.LogInformation($"获取直播间{_liveSettings.RoomId}状态成功，当前状态：{(playInfo.is_living ? "直播中" : "停止直播")}");
                RoomPlayInfo lastPlayInfo = _cache.Get<RoomPlayInfo>(CacheKeyConstant.LIVE_STATUS_KEY);
                if (lastPlayInfo == null)
                {
                    //第一次存缓存强制设置直播状态为1，免得第一次开启时就发送通知
                    playInfo.live_status = 1;
                    _cache.Set(CacheKeyConstant.LIVE_STATUS_KEY, playInfo);
                    return;
                }
                if (lastPlayInfo.is_living != playInfo.is_living)
                {
                    await SendEmailNotice(playInfo);
                    _cache.Set(CacheKeyConstant.LIVE_STATUS_KEY, playInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取直播间信息时发送错误，{ex.Message}");
            }
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task SendEmailNotice(RoomPlayInfo info)
        {
            try
            {
                _logger.LogInformation($"直播间{info.room_id}直播状态发生改变，发送通知邮件");
                String reason = "";
                string cacheKey = string.Format(CacheKeyConstant.MAIL_SEND_CACHE_KEY, info.live_status);
                if (info.is_living)
                {
                    reason = $"开播提醒：\r\n您订阅的直播间(https://live.bilibili.com/{info.room_id})开始直播啦。";
                }
                else
                {
                    reason = $"关闭提醒：\r\n您订阅的直播间(https://live.bilibili.com/{info.room_id})已经停止直播。";
                }
                long lastSendTime = _cache.Get<long>(cacheKey);
                if (TimeUtil.Timestamp() - lastSendTime < 600)
                {
                    _logger.LogWarning($"邮件发送过于频繁，忽略本次发送。{reason}");
                    return;
                }
                var result = await _email.Send("直播订阅通知", reason);
                _cache.Set(cacheKey, TimeUtil.Timestamp());
                if (result.Item1 != SendStatus.Success)
                {
                    throw new Exception(result.Item2);
                }
                _logger.LogInformation("通知邮件已发送！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"发送邮件通知失败，错误：{ex.Message}");
            }
        }
    }
}
