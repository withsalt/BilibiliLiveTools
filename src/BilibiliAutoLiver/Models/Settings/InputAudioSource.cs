using BilibiliAutoLiver.Models.Enums;

namespace BilibiliAutoLiver.Models.Settings
{
    public class InputAudioSource
    {
        public InputType Type { get; set; }

        public int Index { get; set; } = -1;

        public string Path { get; set; }
    }
}
