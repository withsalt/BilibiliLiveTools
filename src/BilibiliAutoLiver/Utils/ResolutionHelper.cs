using System;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

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
            return true;
        }
    }
}
