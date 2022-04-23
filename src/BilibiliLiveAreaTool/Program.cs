using BilibiliLiveCommon.Model;
using BilibiliLiveCommon.Services.Interface;
using BilibiliLiver;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BilibiliLiveAreaTool
{
    internal class AreaToolProgram
    {
        static async Task Main(string[] args)
        {
            using (IHost host = Program.CreateHostBuilder(null).Build())
            {
                IServiceProvider serviceProvider = host.Services;
                var apiService = (IBilibiliLiveApiService)serviceProvider.GetService(typeof(IBilibiliLiveApiService));
                if (apiService == null)
                {
                    throw new Exception("从容器中获取BilibiliLiveApiService服务失败。");
                }
                List<LiveAreaItem> info = await apiService.GetLiveAreas();
                if (info == null)
                {
                    throw new Exception("获取直播分区失败。");
                }

                Console.WriteLine("-------------------------");
                Console.WriteLine(" ID          名称    ");

                foreach (var bigCate in info)
                {
                    Console.WriteLine("-------------------------");
                    Console.WriteLine(bigCate.name);
                    Console.WriteLine("-------------------------");
                    foreach (var item in bigCate.list)
                    {
                        Console.WriteLine(String.Format("{0,-6} | {1,-20} ", item.id, item.name));
                    }
                    Console.WriteLine();
                }

                if (args != null && args.Length > 1)
                {
                    string path = null;
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i].ToLower() == "-save" && i < args.Length - 1)
                        {
                            path = args[i + 1];
                            (bool, string) result = OutputToMarkdownFile(info, path);
                            if (result.Item1)
                            {
                                Console.WriteLine($"\n写入到文件【{result.Item2}】成功。");
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

            static (bool, string) OutputToMarkdownFile(List<LiveAreaItem> data, string filePath)
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
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("### 直播间分区信息");
                    sb.AppendLine();
                    sb.AppendLine("|  AreaId | 分类名称  | 分区名称  |");
                    sb.AppendLine("| :------------ | :------------ | :------------ |");
                    foreach (var bigCate in data)
                    {
                        foreach (var item in bigCate.list)
                        {
                            sb.AppendLine($" | {item.id} | {item.name} | {item.parent_name} | ");
                        }
                    }
                    File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                    return (true, filePath);
                }
                catch (Exception ex)
                {
                    return (false, ex.Message);
                }
            }
        }
    }
}
