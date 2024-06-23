using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BilibiliAutoLiver.Utils
{
    public static class CmdAnalyzer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cmd">需要解析的命令</param>
        /// <param name="strictMode"></param>
        /// <param name="message"></param>
        /// <param name="ffmpegCmd"></param>
        /// <returns></returns>
        public static bool TryParse(string cmd, bool strictMode, string materialPath, string url, out string message, out string ffmpegCmd, out string cmdName, out string cmdArgs)
        {
            message = ffmpegCmd = cmdName = cmdArgs = null;
            try
            {
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
                    throw new Exception("代码不规范，程序两行泪（没有有效的推流命令）");
                }
                var ffmpegCmdLines = cmdLines.Where(p => IsAllowedCmdLine(p, strictMode)).ToArray();
                if (ffmpegCmdLines.Length == 0)
                {
                    throw new Exception(strictMode ? "代码不规范，程序两行泪（没有找到ffmpeg开头，且以{URL}结尾的推流命令）" : "代码不规范，程序两行泪（没有找到包含{URL}的推流命令）");
                }
                if (ffmpegCmdLines.Length > 1)
                {
                    throw new Exception("代码不规范，程序两行泪（存在多条有效命令，请注释不需要的推流命令）");
                }
                if (cmdLines.Count(p => !p.StartsWith("//") && !IsAllowedCmdLine(p, strictMode)) >= 1)
                {
                    throw new Exception("代码不规范，程序两行泪（存在无法解析的命令，建议用‘//’进行注释）");
                }
                string cmdLine = MaterialPathParser(ffmpegCmdLines[0], materialPath);
                cmdLine = cmdLine.Replace("{URL}", $"\"{url}\"");

                int firstNullChar = cmdLine.IndexOf(' ');
                if (firstNullChar < 0)
                {
                    throw new Exception("代码不规范，程序两行泪（无法获取命令执行名称，比如在命令ffmpeg -version中，无法获取ffmpeg）");
                }
                cmdName = cmdLine.Substring(0, firstNullChar);
                if (string.IsNullOrEmpty(cmdName))
                {
                    throw new Exception("代码不规范，程序两行泪（命令执行名称不能为空）");
                }
                if (cmdName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    throw new Exception($"代码不规范，程序两行泪（命令执行名称{cmdName}不能包含非法字符）");
                }
                cmdArgs = cmdLine.Substring(firstNullChar)?.Trim(' ');
                if (string.IsNullOrEmpty(cmdArgs))
                {
                    throw new Exception("代码不规范，程序两行泪（参数不能为空)");
                }

                message = "Success";
                ffmpegCmd = cmdLine;
                return true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return false;
            }
        }

        private static bool IsAllowedCmdLine(string cmd, bool strictMode)
        {
            if (strictMode)
            {
                return cmd.StartsWith("ffmpeg", StringComparison.OrdinalIgnoreCase) && cmd.EndsWith("{URL}");
            }
            else
            {
                return cmd.Contains("{URL}");
            }
        }

        private static string MaterialPathParser(string cmdLine, string materialPath)
        {
            if (!cmdLine.Contains("~/"))
            {
                return cmdLine;
            }
            Dictionary<string, string> replacePaths = new Dictionary<string, string>();
            char[] splitChar = new char[] { ',', '|', ' ', '\'', '\"' };
            int index = 0;
            for (int i = cmdLine.IndexOf("~/", index); i > 0 && i < cmdLine.Length - 1 && index < cmdLine.Length; i = cmdLine.IndexOf("~/", index))
            {
                int endIndex = cmdLine.IndexOfAny(splitChar, i + 1);
                if (endIndex == -1 || endIndex <= i || endIndex - i <= 2)
                {
                    index = index + i + 1;
                    if (index >= cmdLine.Length) break;
                    continue;
                }
                string path = cmdLine.Substring(i, endIndex - i);
                string newPath = Path.Combine(materialPath, path.TrimStart('~', '/'));
                string fileFullPath = Path.GetFullPath(newPath);
                //判断是否需要加上双引号
                if (i - 1 > 0 && cmdLine[i - 1] != '\"' && i + path.Length < cmdLine.Length && cmdLine[i + path.Length] != '\"')
                {
                    int nextSpaceIndex = cmdLine.IndexOf(' ', i + path.Length);
                    int beforeSpaceIndex = cmdLine.LastIndexOf(' ', i);
                    if (beforeSpaceIndex > 0 && cmdLine[beforeSpaceIndex + 1] != '\"' && nextSpaceIndex > 0 && nextSpaceIndex < cmdLine.Length && cmdLine[nextSpaceIndex - 1] != '\"')
                    {
                        fileFullPath = $"\"{fileFullPath}\"";
                    }
                }
                replacePaths[path] = fileFullPath;
                index = i + path.Length;
            }
            if (replacePaths.Count > 0)
            {
                foreach (var item in replacePaths)
                {
                    if (!File.Exists(item.Value.Trim('\"')))
                    {
                        throw new Exception($"素材{item.Value}不存在");
                    }
                    cmdLine = cmdLine.Replace(item.Key, item.Value);
                }
            }
            return cmdLine;
        }
    }
}
