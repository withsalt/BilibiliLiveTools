using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bilibili.AspNetCore.Apis.Interface;
using Bilibili.AspNetCore.Apis.Models.Base;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Dtos.FileUpload;
using BilibiliAutoLiver.Models.Entities;
using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Models.Settings;
using BilibiliAutoLiver.Models.ViewModels;
using BilibiliAutoLiver.Repository.Interface;
using DJT.Data.Model.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        private readonly IMaterialRepository _repository;
        private readonly AppSettings _appSettings;

        public MaterialController(ILogger<MaterialController> logger
            , IMemoryCache cache
            , IBilibiliAccountApiService accountService
            , IBilibiliCookieService cookieService
            , IBilibiliLiveApiService liveApiService
            , IMaterialRepository repository
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
            MaterialIndexPageViewModel model = new MaterialIndexPageViewModel()
            {
                FileTypes = EnumExtensions.GetEnumDescriptions<FileType>(0),
            };
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(MaterialListPageRequest request)
        {
            QuaryPageModel<MaterialDto> materials = await _repository.GetPageAsync(request);
            if (materials == null)
            {
                return Json(new ResultModel<QuaryPageModel<MaterialDto>>(-1, "加载数据失败。"));
            }
            return Json(new ResultModel<QuaryPageModel<MaterialDto>>()
            {
                Code = 0,
                Message = "Success",
                Data = materials
            });
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Delete(List<long> ids)
        {
            try
            {
                if (ids?.Any() != true)
                {
                    throw new Exception("文件ID不能为空。");
                }
                var items = await _repository.Where(p => ids.Contains(p.Id)).ToListAsync();
                if (items?.Any() != true)
                {
                    throw new Exception("查找文件信息失败。");
                }
                int result = await _repository.DeleteAsync(p => ids.Contains(p.Id));
                if (result <= 0)
                {
                    throw new Exception("删除文件失败。");
                }
                foreach (var item in items)
                {
                    string filePath = Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory, item.Path);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                return Json(new ResultModel<string>(0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Delete file by id({JsonConvert.SerializeObject(ids)}) failed. {ex.Message}");
                return Json(new ResultModel<string>(-1, ex.Message));
            }
        }

        /// <summary>
        /// 通过id下载文件
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Download(long id)
        {
            try
            {
                if (id <= 0)
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "文件ID不能为空。"
                    };
                }
                var material = await _repository.FindAsync(id);
                if (material == null)
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "获取文件信息失败。"
                    };
                }
                string path = Path.Combine(_appSettings.DataDirectory, GlobalConfigConstant.DefaultMediaDirectory, material.Path);
                if (!System.IO.File.Exists(path))
                {
                    return new ContentResult()
                    {
                        StatusCode = 404,
                        Content = "当前文件不存在。"
                    };
                }
                string fileName = Path.GetFileName(path);
                if (!new FileExtensionContentTypeProvider().TryGetContentType(fileName, out string contentType))
                {
                    contentType = "application/octet-stream";
                }
                return PhysicalFile(Path.GetFullPath(path), contentType, material.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Download file failed. {ex.Message}");
                return new ContentResult()
                {
                    StatusCode = 404,
                    Content = $"获取文件失败，错误：{ex.Message}"
                };
            }
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
            List<Material> materials = new List<Material>();
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

                Material material = new Material()
                {
                    Name = fileName,
                    Path = fileSaveResult.Item1,
                    FileType = fileType,
                    Size = new FileInfo(fileSaveResult.Item2).Length / 1024,
                    CreatedTime = DateTime.UtcNow,
                    CreatedUserId = GlobalConfigConstant.SYS_USERID
                };
                materials.Add(material);
            }
            await _repository.InsertAsync(materials);
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
