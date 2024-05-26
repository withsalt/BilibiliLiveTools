using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using PiPlayer.Models.ViewModels.Response.FileUpload;

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

        public MaterialController(ILogger<MaterialController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , ILiveSettingRepository repository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _cookieService = cookieService ?? throw new ArgumentNullException(nameof(cookieService));
            _liveApiService = liveApiService ?? throw new ArgumentNullException(nameof(liveApiService));
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
                return Json(new UploadResult()
                {
                    error = "上传文件为空。"
                });
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
                    return Json(new UploadResult()
                    {
                        error = "上传文件文件名不能为空。"
                    });
                }
                string ext = Path.GetExtension(fileName);
                if (string.IsNullOrEmpty(ext))
                {
                    return Json(new UploadResult()
                    {
                        error = "后缀名不能为空。"
                    });
                }
                //FileType fileType = FileType.Unknow;
                //if (_config.AppSettings.AllowExtensions != null && _config.AppSettings.AllowExtensions.Count > 0)
                //{
                //    foreach (var fileTypeItem in _config.AppSettings.AllowExtensions)
                //    {
                //        if (fileTypeItem.Values != null && fileTypeItem.Values.Contains(ext.ToLower()))
                //        {
                //            fileType = fileTypeItem.Type;
                //            break;
                //        }
                //    }
                //}
                //if (fileType == FileType.Unknow)
                //{
                //    return Json(new UploadResult()
                //    {
                //        error = "不支持的文件后后缀名。"
                //    });
                //}

                ext = ext.ToLower();
                string fileNewName = Guid.NewGuid().ToString("n") + ext;
                (string, string) fileSaveResult = await SaveFile(item, fileNewName);
                if (string.IsNullOrEmpty(fileSaveResult.Item1))
                {
                    return Json(new UploadResult()
                    {
                        error = "保存文件失败。"
                    });
                }
            }

            //上传成功
            return Json(new UploadResult());
        }

        private async Task<(string, string)> SaveFile(IFormFile formFile, string fileNewName)
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
                string dataPath = Path.Combine("./data/upload", DateTime.Now.ToString("yyyyMMdd"));
                if (!Directory.Exists(dataPath))
                {
                    Directory.CreateDirectory(dataPath);
                }
                string fileName = Path.Combine(dataPath, fileNewName);
                if (System.IO.File.Exists(fileName))
                {
                    System.IO.File.Delete(fileName);
                }
                using (var fileStream = new FileStream(fileName, FileMode.Create))
                {
                    var inputStream = formFile.OpenReadStream();
                    await inputStream.CopyToAsync(fileStream, 80 * 1024, default);
                }
                if (System.IO.File.Exists(fileName))
                {
                    return (Path.Combine(dataPath, fileNewName).Replace(Path.DirectorySeparatorChar, '/'), fileName);
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
