using System.IO;
using BilibiliAutoLiver.Config;
using BilibiliAutoLiver.Models.Entities;

namespace BilibiliAutoLiver.Utils
{
    public static class MaterialPath
    {
        /// <summary>
        /// 获取素材绝对路径
        /// </summary>
        /// <param name="material"></param>
        /// <param name="dataDirectory"></param>
        /// <returns></returns>
        public static string GetAbsolutePath(Material material, string dataDirectory)
        {
            string path = Path.Combine(dataDirectory, GlobalConfigConstant.DefaultMediaDirectory, material.Path);
            return Path.GetFullPath(path);
        }
    }
}
