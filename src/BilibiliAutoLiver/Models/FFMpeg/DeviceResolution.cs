namespace BilibiliAutoLiver.Models.FFMpeg
{
    public class DeviceResolution
    {
        public DeviceResolution(string format, int width, int height)
        {
            this.Format = format;
            this.Width = width;
            this.Height = height;
        }

        public string Format {  get; set; }

        public int Width {  get; set; }

        public int Height {  get; set; }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}
