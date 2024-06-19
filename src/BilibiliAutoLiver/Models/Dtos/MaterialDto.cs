using System;

namespace BilibiliAutoLiver.Models.Dtos
{
    public class MaterialDto
    {
        public long Id { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 文件大小（KB）
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// 文件类型
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreatedTime { get; set; }
    }
}
