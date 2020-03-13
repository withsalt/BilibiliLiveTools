using Bilibili.Api;
using Bilibili.Model.Live.LiveCategoryInfo;
using Bilibili.Model.Live.LiveRoomInfo;
using Bilibili.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiveCategoryList
{
    class Program
    {
        /// <summary>
        /// 获取全部分类
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task Main(string[] args)
        {
            try
            {
                GlobalSettings.LoadAll();
                LiveCategoryDataInfo info = await LiveApi.GetLiveCategoryInfo();
                if (info != null && info.Code == 0)
                {
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(" ID          名称    ");
                    foreach (var bigCate in info.Data)
                    {
                        Console.WriteLine("-------------------------");
                        Console.WriteLine(bigCate.Name);
                        Console.WriteLine("-------------------------");
                        foreach (var item in bigCate.List)
                        {
                            Console.WriteLine(String.Format("{0,-6} | {1,-20} ", item.id, item.name));
                        }
                    }

                    if (args != null && args.Length > 1)
                    {
                        string path = null;
                        for (int i = 0; i < args.Length; i++)
                        {
                            if (args[i].ToLower() == "-md" && i < args.Length - 1)
                            {
                                path = args[i + 1];
                                (bool, string) result = OutputToMarkdownFile(info.Data, path);
                                if (result.Item1)
                                {
                                    Console.WriteLine("\n写入到文件成功。");
                                }
                                else
                                {
                                    Console.WriteLine($"\n写入到文件失败。{result.Item2}");
                                }
                            }
                        }
                    }

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.ReadKey(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static (bool, string) OutputToMarkdownFile(List<BigCategoryItem> data, string filePath)
        {
            try
            {
                if (data == null || data.Count == 0)
                {
                    return (false, "数据为空。");
                }
                if (string.IsNullOrEmpty(filePath))
                {
                    return (false, "文件目录不存在。");
                }
                filePath = Path.GetFullPath(filePath);
                string path = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("|  ID | 分类名称  | 分区名称  |");
                sb.AppendLine("| :------------ | :------------ | :------------ |");
                foreach (var bigCate in data)
                {
                    foreach (var item in bigCate.List)
                    {
                        sb.AppendLine($" | {item.id} | {item.name} | {item.parent_name} | ");
                    }
                }
                File.WriteAllText(filePath, sb.ToString());
                return (true, "成功。");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
