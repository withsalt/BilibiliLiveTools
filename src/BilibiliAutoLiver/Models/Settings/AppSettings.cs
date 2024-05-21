using System;
using System.IO;
using System.Runtime.InteropServices;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Settings
{
    public class AppSettings
    {
        public static string Position { get { return "AppSettings"; } }

        private string _dataDirectory = null;

        public string DataDirectory
        {
            get
            {
                return _dataDirectory;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(DataDirectory), "Data directory can not null");
                }

                string dic = value;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    dic = dic.Replace('/', Path.DirectorySeparatorChar);
                else
                    dic = dic.Replace('\\', Path.DirectorySeparatorChar);

                if (CommonHelper.TryParseLocalPathString(dic, "{BaseDirectory}", AppContext.BaseDirectory, out string connTemp))
                {
                    dic = connTemp;
                }
                _dataDirectory = dic;
            }
        }

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
    }
}
