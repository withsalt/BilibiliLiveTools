using System;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Settings
{
    public class InputVideoSource
    {
        public InputSourceType Type { get; set; }

        public int Index { get; set; } = -1;

        public string Path { get; set; }

        public string Resolution { get; set; }

        public bool IsMute { get; set; }

        public int Width
        {
            get
            {
                return GetResolution().width;
            }
        }

        public int Height
        {
            get
            {
                return GetResolution().height;
            }
        }

        private (int width, int height) GetResolution()
        {
            if (this.Type != InputSourceType.Device)
            {
                return (0, 0);
            }
            if (string.IsNullOrEmpty(this.Resolution))
            {
                throw new ArgumentException("The resolution is null.");
            }
            char spChar = this.Resolution.Contains('x') ? 'x' : (this.Resolution.Contains('*') ? '*' : throw new Exception($"Can't find a separator with {this.Resolution}"));
            string[] parts = this.Resolution.Split(spChar, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"{this.Resolution} not a standard resolution format.");
            }
            if (!int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
            {
                throw new ArgumentException($"{this.Resolution} not a standard resolution format.");
            }
            return (width, height);
        }
    }
}
