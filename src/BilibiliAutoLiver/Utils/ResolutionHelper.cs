using System;
using FlashCap;

namespace BilibiliAutoLiver.Utils
{
    public static class ResolutionHelper
    {
        public static (int width, int height) Analyze(string resolution)
        {
            if (string.IsNullOrEmpty(resolution))
            {
                throw new ArgumentException("The resolution is null.");
            }
            char spChar = resolution.Contains('x') ? 'x' : (resolution.Contains('*') ? '*' : throw new Exception($"Can't find a separator with {resolution}"));
            string[] parts = resolution.Split(spChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"{resolution} not a standard resolution format.");
            }
            if (!int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
            {
                throw new ArgumentException($"{resolution} not a standard resolution format.");
            }
            return (width, height);
        }

        public static bool TryParse(string resolution, out int width, out int height)
        {
            width = 0;
            height = 0;
            if (string.IsNullOrEmpty(resolution))
            {
                return false;
            }
            char spChar = resolution.Contains('x') ? 'x' : (resolution.Contains('*') ? '*' : throw new Exception($"Can't find a separator with {resolution}"));
            string[] parts = resolution.Split(spChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }
            if (!int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height))
            {
                return false;
            }
            if (width <= 0 || height <= 0)
            {
                return false;
            }
            return true;
        }

        public static bool TryParse(string resolution, out PixelFormats format, out int width, out int height, out int frame)
        {
            width = 0;
            height = 0;
            format = PixelFormats.Unknown;
            frame = 0;
            if (string.IsNullOrEmpty(resolution))
            {
                return false;
            }
            int firstIndex = resolution.IndexOf(',');
            if (firstIndex <= 1)
            {
                return false;
            }
            int secIndex = resolution.IndexOf('@');
            if (secIndex <= firstIndex)
            {
                return false;
            }
            string formatStr = resolution.Substring(0, firstIndex);
            string content = resolution.Substring(firstIndex + 1, secIndex - firstIndex - 1);
            string frameStr = resolution.Substring(secIndex + 1);
            if (!Enum.IsDefined(typeof(PixelFormats), formatStr))
            {
                return false;
            }
            if (!Enum.TryParse<PixelFormats>(formatStr, out format))
            {
                return false;
            }
            if (!int.TryParse(frameStr, out frame))
            {
                return false;
            }
            char spChar = content.Contains('x') ? 'x' : (content.Contains('*') ? '*' : throw new Exception($"Can't find a separator with {content}"));
            string[] parts = content.Split(spChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                return false;
            }
            if (!int.TryParse(parts[0], out width) || !int.TryParse(parts[1], out height))
            {
                return false;
            }
            if (width <= 0 || height <= 0)
            {
                return false;
            }
            return true;
        }

        public static bool Equal(string source, string target)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
            {
                return false;
            }
            if (!TryParse(source, out int sourceWidth, out int sourceHeight))
            {
                return false;
            }
            if (!TryParse(target, out int targetWidth, out int targetHeight))
            {
                return false;
            }
            return sourceWidth == targetWidth && sourceHeight == targetHeight && sourceWidth > 0 && sourceHeight > 0;
        }

        public static bool Equal(int sourceWidth, int sourceHeight, string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                return false;
            }
            if (!TryParse(target, out int targetWidth, out int targetHeight))
            {
                return false;
            }
            return sourceWidth == targetWidth && sourceHeight == targetHeight && sourceWidth > 0 && sourceHeight > 0;
        }

        public static bool Equal(int sourceWidth, int sourceHeight, int targetWidth, int targetHeight)
        {
            return sourceWidth == targetWidth && sourceHeight == targetHeight && sourceWidth > 0 && sourceHeight > 0;
        }
    }
}
