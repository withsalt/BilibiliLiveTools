using Bilibili.Api;
using Bilibili.Model.Live.LiveCategoryInfo;
using Bilibili.Settings;
using System;
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
                        Console.WriteLine($"分区：{bigCate.Name}");
                        Console.WriteLine("-------------------------");
                        foreach (var item in bigCate.List)
                        {
                            Console.WriteLine(String.Format("{0,-6} | {1,-20} ", item.id, item.name));
                        }
                    }

                    //生成GITHUB中表格
                    //foreach (var bigCate in info.Data)
                    //{
                    //    foreach (var item in bigCate.List)
                    //    {
                    //        Console.WriteLine($" | {item.id} | {item.name} | {item.parent_name} | ");
                    //    }
                    //}
                }

                Console.ReadKey(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
