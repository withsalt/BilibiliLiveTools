using System;
using System.IO;

namespace BilibiliAutoLiver.Utils
{
    public class CommonHelper
    {
        public static bool TryParseLocalPathString(string source, string character, string characterDic, out string connectionString)
        {
            connectionString = null;
            var index = source.IndexOf(character, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
            {
                return false;
            }
            if (source[index + character.Length] == Path.DirectorySeparatorChar)
            {
                connectionString = source.Replace(character, characterDic.TrimEnd(Path.DirectorySeparatorChar));
            }
            else
            {
                connectionString = source.Replace(character, characterDic);
            }
            return true;
        }

        public static (string format, string deviceName) GetDeviceFormatAndName(string device)
        {
            int index = device.IndexOf(',');
            if (index < 0)
            {
                throw new Exception("设备名称不符合规范，设备名称格式：[格式],[设备名称]");
            }
            string format = device.Substring(0, index);
            string name = device.Substring(index + 1);
            return (format, name);
        }
    }
}
