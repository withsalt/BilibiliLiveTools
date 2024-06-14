using System;
using System.Linq;

namespace BilibiliAutoLiver.Utils
{
    public static class CmdAnalyzer
    {
        public static bool TryParse(string cmd, out string message, out string ffmpegCmd)
        {
            message = null;
            ffmpegCmd = null;
            //解析推流命令 
            var cmdLines = cmd
                .ReplaceLineEndings()
                .Split(Environment.NewLine)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim(' ', '\r', '\n'))
                .Where(p => !string.IsNullOrWhiteSpace(p) && !p.StartsWith("//"))
                .ToList();
            if (!cmdLines.Any())
            {
                message = "代码不规范，程序两行泪（没有找到ffmpeg开头，且以[[URL]]结尾的推流命令）";
                return false;
            }
            var ffmpegCmdLines = cmdLines.Where(p => p.StartsWith("ffmpeg", StringComparison.OrdinalIgnoreCase) && p.EndsWith("[[URL]]")).ToArray();
            if (ffmpegCmdLines.Length == 0)
            {
                message = "代码不规范，程序两行泪（没有找到ffmpeg开头，且以[[URL]]结尾的推流命令）";
                return false;
            }
            if (ffmpegCmdLines.Length > 1)
            {
                message = "代码不规范，程序两行泪（存在多条ffmpeg命令，请注释不需要的推流命令）";
                return false;
            }
            if (cmdLines.Count(p => !p.StartsWith("//") && !(p.StartsWith("ffmpeg", StringComparison.OrdinalIgnoreCase) && p.EndsWith("[[URL]]"))) >= 1)
            {
                message = "代码不规范，程序两行泪（存在无法解析的命令，建议用‘//’进行注释）";
                return false;
            }
            message = "Success";
            ffmpegCmd = ffmpegCmdLines[0];
            return true;
        }
    }
}
