using BilibiliAutoLiver.Models.Enums;
using BilibiliAutoLiver.Utils;

namespace BilibiliAutoLiver.Models.Settings
{
    public class InputVideoSource
    {
        public InputType Type { get; set; }

        public string Path { get; set; }

        public string Resolution { get; set; }

        public int Framerate { get; set; }

        public bool IsMute { get; set; }

        public int Width
        {
            get
            {
                if (this.Type != InputType.Camera)
                {
                    return 0;
                }
                return ResolutionHelper.Analyze(this.Resolution).width;
            }
        }

        public int Height
        {
            get
            {
                if (this.Type != InputType.Camera)
                {
                    return 0;
                }
                return ResolutionHelper.Analyze(this.Resolution).height;
            }
        }
    }
}
