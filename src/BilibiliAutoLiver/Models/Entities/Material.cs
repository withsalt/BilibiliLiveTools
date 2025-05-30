﻿using System;
using BilibiliAutoLiver.Extensions;
using BilibiliAutoLiver.Models.Dtos;
using BilibiliAutoLiver.Models.Enums;
using FreeSql.DataAnnotations;
using Newtonsoft.Json;

namespace BilibiliAutoLiver.Models.Entities
{
    public class Material : IBaseEntity
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        [Column(StringLength = 255, IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        [Column(StringLength = 512)]
        public string Path { get; set; }

        /// <summary>
        /// 文件大小（KB）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public FileType FileType { get; set; }

        public string Description { get; set; }

        public string MediaInfo { get; set; }

        public MaterialDto ToDto(string basePath)
        {
            string path = System.IO.Path.Combine(basePath, Path);
            string fullPath = System.IO.Path.GetFullPath(path);

            var dto = new MaterialDto()
            {
                Id = Id,
                Name = Name,
                Size = ConvertFileSize(Size),
                Duration = "",
                Path = $"~/{this.Path}",
                FullPath = fullPath,
                FileType = EnumExtensions.GetEnumDescription(FileType),
                Description = Description,
                MediaInfo = this.GetMediaInfo(),
                CreatedTime = CreatedTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")
            };
            if (dto.MediaInfo != null)
            {
                dto.Duration = TimeSpan.FromSeconds(dto.MediaInfo.Duration).ToString(@"hh\:mm\:ss");
            }
            return dto;
        }

        public MediaInfo GetMediaInfo()
        {
            if (!string.IsNullOrWhiteSpace(this.MediaInfo))
            {
                return JsonConvert.DeserializeObject<MediaInfo>(this.MediaInfo);
            }
            return null;
        }

        private string ConvertFileSize(long fileSizeInBytes)
        {
            string[] sizes = { "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = fileSizeInBytes;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }
    }
}
