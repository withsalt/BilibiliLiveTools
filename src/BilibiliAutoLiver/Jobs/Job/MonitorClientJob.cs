using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Utils;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Jobs.Interface;
using BilibiliAutoLiver.Models;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BilibiliAutoLiver.Jobs.Job
{
    [DisallowConcurrentExecution]
    public class MonitorClientJob : BaseJobDescribe, IJob
    {
        private readonly ILogger<MonitorClientJob> _logger;
        private readonly IMemoryCache _cache;
        //private readonly IEmailNoticeService _email;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly IServiceProvider _provider;

        public MonitorClientJob(ILogger<MonitorClientJob> logger
            , IMemoryCache cache
            //, IEmailNoticeService email
            , IBilibiliLiveApiService liveApiService
            , IServiceProvider provider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            //_email = email ?? throw new ArgumentNullException(nameof(email));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                MonitorSetting setting = await _provider.GetRequiredService<IMonitorSettingRepository>().GetCacheAsync();
                if (setting == null || !setting.IsEnabled)
                {
                    if (setting != null)
                    {
                        _cache.Remove(CacheKeyConstant.LIVE_STATUS_CACHE_KEY);
                        _cache.Remove(string.Format(CacheKeyConstant.LIVE_LOGS_CACHE_KEY, setting.RoomId));
                    }
                    return;
                }

                _logger.LogInformation($"开始查询直播间{setting.RoomId}状态....");
                if (setting.RoomId <= 0)
                {
                    throw new Exception("获取需要查询的直播房间号失败，请检查配置文件是否正确配置房间号。");
                }
                LiveRoomInfo playInfo = await _liveApiService.GetLiveRoomInfo(setting.RoomId);
                if (playInfo == null || playInfo.room_id != setting.RoomId)
                {
                    throw new Exception("获取直播间信息失败。");
                }
                Log(setting.RoomId, playInfo);
                LiveRoomInfo lastPlayInfo = _cache.Get<LiveRoomInfo>(CacheKeyConstant.LIVE_STATUS_CACHE_KEY);
                if (lastPlayInfo == null)
                {
                    lastPlayInfo = playInfo;
                    _cache.Set(CacheKeyConstant.LIVE_STATUS_CACHE_KEY, playInfo);
                    return;
                }
                if (lastPlayInfo.is_living != playInfo.is_living)
                {
                    await SendEmailNotice(playInfo);
                    _cache.Set(CacheKeyConstant.LIVE_STATUS_CACHE_KEY, playInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取直播间信息时发送错误，{ex.Message}");
            }
        }

        private void Log(long roomId, LiveRoomInfo playInfo)
        {
            _logger.LogInformation($"获取直播间{roomId}状态成功，当前状态：{(playInfo.is_living ? "直播中" : "停止直播")}");
            string key = string.Format(CacheKeyConstant.LIVE_LOGS_CACHE_KEY, roomId);
            Queue<string> queue = _cache.Get<Queue<string>>(key);
            if (queue == null)
            {
                queue = new Queue<string>(30);
            }
            string log = $"【{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}】直播间：{playInfo.title}，当前状态：{(playInfo.is_living ? "直播中" : "停止直播")}";
            queue.Enqueue(log);
            if (queue.Count > 6)
            {
                _ = queue.Dequeue();
            }
            _cache.Set(key, queue);
        }

        /// <summary>
        /// 发送通知
        /// </summary>
        /// <param name="reason"></param>
        /// <returns></returns>
        private async Task SendEmailNotice(LiveRoomInfo info)
        {
            try
            {
                _logger.LogInformation($"直播间{info.title}({info.room_id})直播状态发生改变，发送通知邮件");
                string reason = info.is_living ? $"开播提醒：\r\n您订阅的直播间{info.title}(https://live.bilibili.com/{info.room_id})开始直播啦。"
                    : $"关闭提醒：\r\n您订阅的直播间{info.title}(https://live.bilibili.com/{info.room_id})已经停止直播。";
                string cacheKey = string.Format(CacheKeyConstant.MAIL_SEND_CACHE_KEY, info.live_status);
                long lastSendTime = _cache.Get<long>(cacheKey);
                if (TimeUtil.Timestamp() - lastSendTime < 600)
                {
                    _logger.LogWarning($"邮件发送过于频繁，忽略本次发送。{reason}");
                    return;
                }
                IEmailNoticeService email = _provider.GetRequiredService<IEmailNoticeService>();
                var result = await email.Send("直播订阅通知", reason);
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

        #region Job info

        private JobMetadata _jobMetadata = null;

        protected override JobMetadata JobMetadata
        {
            get
            {
                if (_jobMetadata != null)
                {
                    return _jobMetadata;
                }
                _jobMetadata = new JobMetadata()
                {
                    Id = 3,
                    Name = $"{this.GetType().Name}Service",
                    IntervalTime = 30,
                    StartTime = DateTime.Now.AddSeconds(10)
                };
                return _jobMetadata;
            }
        }

        #endregion
    }
}
