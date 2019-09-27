using Bilibili;
using Bilibili.Api;
using Bilibili.Helper;
using Bilibili.Model.Live.StartLiveInfo;
using Bilibili.Settings;
using BilibiliLiveTools.Model;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace BilibiliLiveTools
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string usersFilePath;
            Users users;

            Console.Title = GetTitle();
            GlobalSettings.Logger = Logger.Instance;
            if (!BitConverter.IsLittleEndian)
            {
                GlobalSettings.Logger.LogWarning("在BigEndian模式的CPU下工作可能导致程序出错");
                GlobalSettings.Logger.LogWarning("如果出现错误，请创建issue");
            }
            usersFilePath = Path.Combine(Environment.CurrentDirectory, "Settings", "Users.json");
            try
            {
                GlobalSettings.LoadAll();
                users = Users.FromJson(File.ReadAllText(usersFilePath));
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogException(ex);
                GlobalSettings.Logger.LogError($"缺失或无效配置文件，请检查是否添加\"{usersFilePath}\"");
                Console.ReadKey(true);
                return;
            }
            LoginApiExtensions.LoginDataUpdated += (sender, e) => File.WriteAllText(usersFilePath, users.ToJson());
            User user = users[0];
            if (!await user.Login())
            {
                string cookie = user.Data.Cookie;
                GlobalSettings.Logger.LogError($"账号{user.Account}登录失败！");
                Console.ReadKey(true);
                return;
            }

            try
            {
                LiveSetting liveSetting = LoadLiveSettingConfig();

                //获取直播房间号
                LiveApi liveApi = new LiveApi();
                StartLiveDataInfo liveInfo = await liveApi.StartLive(user, liveSetting.LiveCategory);
                if (liveInfo != null)
                {
                    GlobalSettings.Logger.LogInfo("开启直播成功！");
                    GlobalSettings.Logger.LogInfo($"我的直播间地址：http://live.bilibili.com/{liveInfo.RoomId}");
                    GlobalSettings.Logger.LogInfo($"推流地址：{liveInfo.Rtmp.Addr}{liveInfo.Rtmp.Code}");
                }
            }
            catch (Exception ex)
            {
                GlobalSettings.Logger.LogError($"开启直播失败！错误：{ex.Message}");
            }

            Console.ReadKey(true);
        }

        private static string GetTitle()
        {
            string productName = GetAssemblyAttribute<AssemblyProductAttribute>().Product;
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return $"{productName} v{version}";
        }

        private static T GetAssemblyAttribute<T>()
        {
            return (T)Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false)[0];
        }

        /// <summary>
        /// 自动从文件加载配置文件
        /// </summary>
        /// <param name="configFileName">配置文件名称，位于程序根目录，默认使用appsettings.json</param>
        /// <returns></returns>
        private static LiveSetting LoadLiveSettingConfig()
        {
            try
            {
                string configPath = Path.Combine(AppContext.BaseDirectory, "Settings", "LiveSetting.json");
                if (!File.Exists(configPath))
                {
                    throw new Exception("File 'LiveSetting.json' not exist.");
                }
                string loadConfigString = File.ReadAllText(configPath);
                if (string.IsNullOrEmpty(loadConfigString))
                {
                    throw new Exception("Read file 'LiveSetting.json' failed.");
                }
                LiveSetting config = JsonHelper.DeserializeJsonToObject<LiveSetting>(loadConfigString);
                return config;
            }
            catch (Exception ex)
            {
                throw new Exception($"Load config failed. {ex.Message}");
            }
        }
    }
}
