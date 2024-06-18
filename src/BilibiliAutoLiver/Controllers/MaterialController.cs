using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos.FileUpload;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BilibiliAutoLiver.Controllers
{
    [Authorize]
    public class MaterialController : Controller
    {
        private readonly ILogger<MaterialController> _logger;
        private readonly IMemoryCache _cache;
        private readonly IBilibiliAccountApiService _accountService;
        private readonly IBilibiliCookieService _cookieService;
        private readonly IBilibiliLiveApiService _liveApiService;
        private readonly ILiveSettingRepository _repository;
        private readonly AppSettings _appSettings;

        public MaterialController(ILogger<MaterialController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , ILiveSettingRepository repository
            , IOptions<AppSettings> settingOptions)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _appSettings = settingOptions?.Value ?? throw new ArgumentNullException(nameof(settingOptions));
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(UploadParams uploadParams)
        {
            IFormFileCollection files = Request.Form.Files;
            if (files == null || files.Count == 0)
            {
                return Json(new ResultModel<string>(-1, "上传文件为空"));
            }
            foreach (var item in files)
            {
                uploadParams.uploadFiles.Add(item);
            }
            foreach (var item in uploadParams.uploadFiles)
            {
                string fileName = item.FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    return Json(new ResultModel<string>(-1, "上传文件文件名不能为空"));
                }
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(ext))
                {
                    return Json(new ResultModel<string>(-1, "后缀名不能为空"));
                }
                FileType fileType = FileType.Unknow;
                if (_appSettings.AllowExtensions != null && _appSettings.AllowExtensions.Count > 0)
                {
                    foreach (var fileTypeItem in _appSettings.AllowExtensions)
                    {
                        if (fileTypeItem.Values != null && fileTypeItem.Values.Contains(ext.ToLower()))
                        {
                            fileType = fileTypeItem.Type;
                            break;
                        }
                    }
                }
                if (fileType == FileType.Unknow)
                {
                    return Json(new ResultModel<string>(-1, $"不支持的文件类型：{ext.TrimStart('.')}"));
                }

                ext = ext.ToLower();
                string fileNewName = Guid.NewGuid().ToString("n") + ext;
                (string, string) fileSaveResult = await SaveFile(item, fileNewName);
                if (string.IsNullOrEmpty(fileSaveResult.Item1))
                {
                    return Json(new ResultModel<string>(-1, "保存文件失败"));
                }
            }

            //上传成功
            return Json(new ResultModel<string>(0));
        }

        private async Task<(string relativePath, string absolutePath)> SaveFile(IFormFile formFile, string fileNewName)
        {
            try
            {
                if (formFile == null)
                {
                    return (null, null);
                }
                if (string.IsNullOrEmpty(fileNewName))
                {
                    return (null, null);
                }
                string relativePath = Path.Combine(DateTime.Now.ToString("yyyyMMdd"), fileNewName);
                string absolutePath = Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory, relativePath);
                string filePath = Path.GetDirectoryName(absolutePath);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                if (System.IO.File.Exists(absolutePath))
                {
                    System.IO.File.Delete(absolutePath);
                }
                using (var fileStream = new FileStream(absolutePath, FileMode.Create))
                {
                    var inputStream = formFile.OpenReadStream();
                    await inputStream.CopyToAsync(fileStream, 80 * 1024, default);
                }
                if (System.IO.File.Exists(absolutePath))
                {
                    return (relativePath.Replace(Path.DirectorySeparatorChar, '/'), absolutePath);
                }
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Save upload file failed. {ex.Message}");
                return (null, null);
            }
        }

    }
}
