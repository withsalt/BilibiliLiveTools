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
    }
}
