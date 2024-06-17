using System;
using System.IO;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Settings
{
    public class AppSettings
    {
        public static string Position { get { return "AppSettings"; } }

        private string _connectionString = null;

        /// <summary>
        /// 连接字符串
        /// </summary>
        public string DbConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DbConnectionString), "Connection string can not null");
                }
                string connStr = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    connStr = connStr.Replace('/', Path.DirectorySeparatorChar);
                else
                    connStr = connStr.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(connStr, "{BaseDirectory}", AppContext.BaseDirectory, out string connTemp))
                {
                    connStr = connTemp;
                }
                _connectionString = connStr;
            }
        }

        /// <summary>
        /// 高级模式命令解析采用严格模式
        /// </summary>
        public bool AdvanceStrictMode { get; set; }
    }
}
