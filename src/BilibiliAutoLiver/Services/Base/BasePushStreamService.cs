using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services.Interface;
using BilibiliAutoLiver.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Services.Base
{
    public abstract class BasePushStreamService : IPushStreamService
    {
        private readonly ILogger _logger;
        private readonly IBilibiliAccountApiService _account;
        private readonly IBilibiliLiveApiService _api;
        private readonly IFFMpegService _ffmpeg;
        private readonly IServiceProvider _serviceProvider;
        private readonly AppSettings _appSettings;

        protected PushStatus Status { get; set; } = PushStatus.Stopped;

        public BasePushStreamService(ILogger logger
            , IBilibiliAccountApiService account
            , IBilibiliLiveApiService api
            , IServiceProvider serviceProvider
            , IFFMpegService ffmpeg
            , AppSettings appSettings)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _account = account ?? throw new ArgumentNullException(nameof(account));
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        }

        public abstract Task<bool> Start();

        public abstract Task<bool> Stop();

        public virtual PushStatus GetStatus()
        {
            return this.Status;
        }

        /// <summary>
        /// 获取推送设置
        /// </summary>
        /// <returns></returns>
        public async Task<SettingDto> GetSetting()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var provider = scope.ServiceProvider;
                PushSetting pushSetting = await provider.GetRequiredService<IPushSettingRepository>().Where(p => !p.IsDeleted).FirstAsync();
                LiveSetting liveSetting = await provider.GetRequiredService<ILiveSettingRepository>().Where(p => !p.IsDeleted).FirstAsync();
                return new SettingDto()
                {
                    LiveSetting = liveSetting,
                    PushSetting = await ConvertPushSettingToDto(scope.ServiceProvider, pushSetting),
                };
            }
        }

        private async Task<PushSettingDto> ConvertPushSettingToDto(IServiceProvider provider, PushSetting pushSetting)
        {
            if (pushSetting == null)
                throw new ArgumentNullException(nameof(pushSetting), "推流设置获取失败");

            if (!pushSetting.IsUpdate)
                return null;

            if (!ResolutionHelper.TryParse(pushSetting.OutputResolution, out int outputWidth, out int outputHeight))
                throw new ArgumentException(pushSetting.OutputResolution, $"输出分辨率格式不正确，{pushSetting.OutputResolution}");

            int inputWidth = 0, inputHeight = 0;

            switch (pushSetting.InputType)
            {
                case InputType.Video:
                    break;
                case InputType.Desktop:
                    break;
                case InputType.Camera:
                    if (!ResolutionHelper.TryParse(pushSetting.InputResolution, out inputWidth, out inputHeight))
                    {
                        throw new ArgumentException(pushSetting.InputResolution, $"输入分辨率格式不正确，{pushSetting.OutputResolution}");
                    }
                    break;
                case InputType.CameraPlus:
                    if (!ResolutionHelper.TryParse(pushSetting.InputResolution, out inputWidth, out inputHeight))
                    {
                        throw new ArgumentException(pushSetting.InputResolution, $"输入分辨率格式不正确，{pushSetting.OutputResolution}");
                    }
                    break;
                default:
                    break;
            }

            var materialRepository = provider.GetRequiredService<IMaterialRepository>();

            List<long> materialIds = new List<long>(2);
            if (pushSetting.VideoId > 0)
            {
                materialIds.Add(pushSetting.VideoId);
            }
            if (pushSetting.AudioId.HasValue && pushSetting.AudioId > 0)
            {
                materialIds.Add(pushSetting.AudioId.Value);
            }
            List<Material> materials = await provider.GetRequiredService<IMaterialRepository>().Where(p => materialIds.Contains(p.Id)).ToListAsync();
            Material videoMaterial = materials.Where(p => p.FileType == FileType.Video).FirstOrDefault();
            if (videoMaterial == null)
            {
                throw new FileNotFoundException($"Id为{pushSetting.VideoId}视频素材不存在！");
            }
            Material audioMaterial = materials.Where(p => p.FileType == FileType.Music).FirstOrDefault();
            PushSettingDto pushSettingDto = new PushSettingDto()
            {
                Key = pushSetting.Key,
                Model = pushSetting.Model,
                Quality = pushSetting.Quality,
                FFmpegCommand = pushSetting.FFmpegCommand,
                InputType = pushSetting.InputType,
                OutputResolution = pushSetting.OutputResolution,
                CustumOutputParams = pushSetting.CustumOutputParams,
                CustumVideoCodec = pushSetting.CustumVideoCodec,
                VideoId = pushSetting.VideoId,
                IsMute = pushSetting.IsMute,
                AudioId = pushSetting.AudioId,
                AudioDevice = pushSetting.AudioDevice,
                DeviceName = pushSetting.DeviceName,
                Plugins = pushSetting.Plugins,
                InputResolution = pushSetting.InputResolution,
                InputFramerate = pushSetting.InputFramerate,
                InputScreen = pushSetting.InputScreen,
                InputAudioSource = pushSetting.InputAudioSource,
                IsAutoRetry = pushSetting.IsAutoRetry,
                RetryInterval = pushSetting.RetryInterval,
                IsUpdate = pushSetting.IsUpdate,

                OutputWidth = outputWidth,
                OutputHeight = outputHeight,
                InputWidth = inputWidth,
                InputHeight = inputHeight,
                VideoMaterial = videoMaterial.ToDto(Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory)),
                AudioMaterial = audioMaterial?.ToDto(Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory)),
                VideoCodecs = _ffmpeg.GetVideoCodecs(),
            };
            return pushSettingDto;
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
                if (!CmdAnalyzer.TryParse(setting.PushSetting.FFmpegCommand, _appSettings.AdvanceStrictMode, Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory), "", out string message, out _, out _, out _))
                {
                    _logger.ThrowLogError(message);
                }
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

        /// <summary>
        /// 网络联通检查
        /// </summary>
        /// <param name="tokenSource"></param>
        /// <returns></returns>
        protected async Task CheckNetwork(CancellationTokenSource tokenSource)
        {
            while (!await NetworkUtil.Ping() && !tokenSource.IsCancellationRequested)
            {
                _logger.LogWarning($"网络连接已断开，将在10秒后重新检查网络连接...");
                await Task.Delay(10000, tokenSource.Token);
            }
        }

        protected void Delay(int sec, CancellationTokenSource tokenSource)
        {
            _logger.LogWarning($"等待{sec}s后重新推流...");
            Stopwatch sw = Stopwatch.StartNew();
            int timeout = sec * 1000;
            while (sw.ElapsedMilliseconds <= timeout && !tokenSource.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }

        public abstract void Dispose();
    }
}
