using System.Reflection;

namespace BilibiliAutoLiver.Utils
{
    public static class VersionHelper
    {
        public static string GetVersion()
        {
            return Assembly.GetAssembly(typeof(Program)).GetName()?.Version?.ToString()?.TrimEnd('.', '0');
        }
    }
}
