using System;
using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models
{
    public class InputAudioSource
    {
        public InputSourceType Type { get; set; }

        public int Index { get; set; } = -1;

        public string Path { get; set; }
    }
}
