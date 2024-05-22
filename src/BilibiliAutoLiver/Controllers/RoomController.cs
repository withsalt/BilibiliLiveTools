using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Dtos.Common;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class RoomController : Controller
    {
        private readonly ILogger<RoomController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly ILiveSettingRepository _liveSettingRepos;

        public RoomController(ILogger<RoomController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , ILiveSettingRepository liveSettingRepos)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _liveSettingRepos = liveSettingRepos ?? throw new ArgumentNullException(nameof(liveSettingRepos));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            MyLiveRoomInfo myLiveRoomInfo = await _liveApiService.GetMyLiveRoomInfo();
            List<LiveAreaItem> liveAreas = await _liveApiService.GetLiveAreas();
            LiveSetting liveSetting = await _liveSettingRepos.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();

            RoomInfoIndexPageViewModel viewModel = new RoomInfoIndexPageViewModel()
            {
                LiveRoomInfo = myLiveRoomInfo,
                LiveAreas = liveAreas,
                LiveSetting = liveSetting
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ResultModel<string>> Update([FromBody] RoomInfoUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new ResultModel<string>(-1, "参数错误");
                }
                //调用B站api，先更新b站直播信息
                LiveSetting liveSetting = await _liveSettingRepos.Where(p => !p.IsDeleted).OrderByDescending(p => p.CreatedTime).FirstAsync();
                if (liveSetting == null || liveSetting.AreaId != request.AreaId)
                {
                    //更新分区
                    if (!await _liveApiService.UpdateLiveRoomArea(request.RoomId, request.AreaId))
                    {
                        return new ResultModel<string>(-1, "更新Bilibili直播分区失败");
                    }
                }
                if (liveSetting == null || liveSetting.RoomName != request.RoomName)
                {
                    //更新直播名称
                    if (!await _liveApiService.UpdateLiveRoomName(request.RoomId, request.RoomName))
                    {
                        return new ResultModel<string>(-1, "更新Bilibili直播名称失败");
                    }
                }
                if (liveSetting == null)
                {
                    liveSetting = new LiveSetting()
                    {
                        AreaId = request.AreaId,
                        RoomName = request.RoomName,
                        IsAutoRetry = request.IsAutoRetry,
                        RetryInterval = request.RetryInterval,
                        CreatedTime = DateTime.Now,
                        CreatedUserId = GlobalConfigConstant.SYS_USERID,
                        UpdatedTime = DateTime.Now,
                        UpdatedUserId = GlobalConfigConstant.SYS_USERID,
                    };
                }
                else
                {
                    liveSetting.AreaId = request.AreaId;
                    liveSetting.RoomName = request.RoomName;
                    liveSetting.IsAutoRetry = request.IsAutoRetry;
                    liveSetting.RetryInterval = request.RetryInterval;
                    liveSetting.UpdatedTime = DateTime.Now;
                    liveSetting.UpdatedUserId = GlobalConfigConstant.SYS_USERID;
                }
                await _liveSettingRepos.InsertOrUpdateAsync(liveSetting);
                return new ResultModel<string>(0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"更新直播间信息失败，{ex.Message}");
                return new ResultModel<string>(-1, ex.Message);
            }
        }
    }
}
