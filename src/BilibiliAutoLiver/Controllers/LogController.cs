using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using BilibiliAutoLiver.Services;
using BilibiliAutoLiver.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class LogController : Controller
    {
        private readonly ILogger<LogController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IFFMpegService _ffmpeg;

        public LogController(ILogger<LogController> logger
            , IMemoryCache cache
            , IFFMpegService ffmpeg)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _ffmpeg = ffmpeg ?? throw new ArgumentNullException(nameof(ffmpeg));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ResultModel<List<LogItemResponse>> GetLogs()
        {
            var allLogs = _ffmpeg.GetLogs();
            if (!allLogs.Any())
            {
                return new ResultModel<List<LogItemResponse>>();
            }
            var logs = allLogs.OrderBy(p => p.Time).Select(p => new LogItemResponse()
            {
                LogType = p.LogType,
                Time = p.Time,
                Message = p.Message,
                StackTrace = p.Exception?.StackTrace ?? string.Empty,
            }).ToList();
            return new ResultModel<List<LogItemResponse>>(0)
            {
                Data = logs,
            };
        }
    }
}
